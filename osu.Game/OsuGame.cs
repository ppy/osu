// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Music;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Toolbar;
using osu.Game.Overlays.Volume;
using osu.Game.Performance;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Skinning;
using osu.Game.Skinning.Editor;
using osu.Game.Updater;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game
{
    /// <summary>
    /// The full osu! experience. Builds on top of <see cref="OsuGameBase"/> to add menus and binding logic
    /// for initial components that are generally retrieved via DI.
    /// </summary>
    public class OsuGame : OsuGameBase, IKeyBindingHandler<GlobalAction>, ILocalUserPlayInfo
    {
        /// <summary>
        /// The amount of global offset to apply when a left/right anchored overlay is displayed (ie. settings or notifications).
        /// </summary>
        protected const float SIDE_OVERLAY_OFFSET_RATIO = 0.05f;

        public Toolbar Toolbar;

        private ChatOverlay chatOverlay;

        private ChannelManager channelManager;

        [NotNull]
        protected readonly NotificationOverlay Notifications = new NotificationOverlay();

        private BeatmapListingOverlay beatmapListing;

        private DashboardOverlay dashboard;

        private NewsOverlay news;

        private UserProfileOverlay userProfile;

        private BeatmapSetOverlay beatmapSetOverlay;

        private WikiOverlay wikiOverlay;

        private ChangelogOverlay changelogOverlay;

        private SkinEditorOverlay skinEditor;

        private Container overlayContent;

        private Container rightFloatingOverlayContent;

        private Container leftFloatingOverlayContent;

        private Container topMostOverlayContent;

        protected ScalingContainer ScreenContainer { get; private set; }

        protected Container ScreenOffsetContainer { get; private set; }

        private Container overlayOffsetContainer;

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; }

        [Cached]
        private readonly DifficultyRecommender difficultyRecommender = new DifficultyRecommender();

        [Cached]
        private readonly LegacyImportManager legacyImportManager = new LegacyImportManager();

        [Cached]
        private readonly ScreenshotManager screenshotManager = new ScreenshotManager();

        protected SentryLogger SentryLogger;

        public virtual StableStorage GetStorageForStableInstall() => null;

        private float toolbarOffset => (Toolbar?.Position.Y ?? 0) + (Toolbar?.DrawHeight ?? 0);

        private IdleTracker idleTracker;

        /// <summary>
        /// Whether overlays should be able to be opened game-wide. Value is sourced from the current active screen.
        /// </summary>
        public readonly IBindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>();

        /// <summary>
        /// Whether the local user is currently interacting with the game in a way that should not be interrupted.
        /// </summary>
        /// <remarks>
        /// This is exclusively managed by <see cref="Player"/>. If other components are mutating this state, a more
        /// resilient method should be used to ensure correct state.
        /// </remarks>
        public Bindable<bool> LocalUserPlaying = new BindableBool();

        protected OsuScreenStack ScreenStack;

        protected BackButton BackButton;

        protected SettingsOverlay Settings;

        private VolumeOverlay volume;
        private OsuLogo osuLogo;

        private MainMenu menuScreen;

        private VersionManager versionManager;

        [CanBeNull]
        private IntroScreen introScreen;

        private Bindable<string> configRuleset;

        private Bindable<float> uiScale;

        private Bindable<string> configSkin;

        private readonly string[] args;

        private readonly List<OsuFocusedOverlayContainer> focusedOverlays = new List<OsuFocusedOverlayContainer>();

        private readonly List<OverlayContainer> visibleBlockingOverlays = new List<OverlayContainer>();

        public OsuGame(string[] args = null)
        {
            this.args = args;

            forwardLoggedErrorsToNotifications();

            SentryLogger = new SentryLogger(this);
        }

        private void updateBlockingOverlayFade() =>
            ScreenContainer.FadeColour(visibleBlockingOverlays.Any() ? OsuColour.Gray(0.5f) : Color4.White, 500, Easing.OutQuint);

        public void AddBlockingOverlay(OverlayContainer overlay)
        {
            if (!visibleBlockingOverlays.Contains(overlay))
                visibleBlockingOverlays.Add(overlay);
            updateBlockingOverlayFade();
        }

        public void RemoveBlockingOverlay(OverlayContainer overlay) => Schedule(() =>
        {
            visibleBlockingOverlays.Remove(overlay);
            updateBlockingOverlayFade();
        });

        /// <summary>
        /// Close all game-wide overlays.
        /// </summary>
        /// <param name="hideToolbar">Whether the toolbar should also be hidden.</param>
        public void CloseAllOverlays(bool hideToolbar = true)
        {
            foreach (var overlay in focusedOverlays)
                overlay.Hide();

            if (hideToolbar) Toolbar.Hide();
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.CacheAs(this);

            dependencies.Cache(SentryLogger);

            dependencies.Cache(osuLogo = new OsuLogo { Alpha = 0 });

            // bind config int to database RulesetInfo
            configRuleset = LocalConfig.GetBindable<string>(OsuSetting.Ruleset);
            uiScale = LocalConfig.GetBindable<float>(OsuSetting.UIScale);

            var preferredRuleset = int.TryParse(configRuleset.Value, out int rulesetId)
                // int parsing can be removed 20220522
                ? RulesetStore.GetRuleset(rulesetId)
                : RulesetStore.GetRuleset(configRuleset.Value);

            try
            {
                Ruleset.Value = preferredRuleset ?? RulesetStore.AvailableRulesets.First();
            }
            catch (Exception e)
            {
                // on startup, a ruleset may be selected which has compatibility issues.
                Logger.Error(e, $@"Failed to switch to preferred ruleset {preferredRuleset}.");
                Ruleset.Value = RulesetStore.AvailableRulesets.First();
            }

            Ruleset.ValueChanged += r => configRuleset.Value = r.NewValue.ShortName;

            // bind config int to database SkinInfo
            configSkin = LocalConfig.GetBindable<string>(OsuSetting.Skin);
            SkinManager.CurrentSkinInfo.ValueChanged += skin => configSkin.Value = skin.NewValue.ID.ToString();
            configSkin.ValueChanged += skinId =>
            {
                Live<SkinInfo> skinInfo = null;

                if (Guid.TryParse(skinId.NewValue, out var guid))
                    skinInfo = SkinManager.Query(s => s.ID == guid);

                if (skinInfo == null)
                {
                    if (guid == SkinInfo.CLASSIC_SKIN)
                        skinInfo = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged();
                }

                SkinManager.CurrentSkinInfo.Value = skinInfo ?? DefaultSkin.CreateInfo().ToLiveUnmanaged();
            };
            configSkin.TriggerChange();

            IsActive.BindValueChanged(active => updateActiveState(active.NewValue), true);

            Audio.AddAdjustment(AdjustableProperty.Volume, inactiveVolumeFade);

            SelectedMods.BindValueChanged(modsChanged);
            Beatmap.BindValueChanged(beatmapChanged, true);
        }

        private ExternalLinkOpener externalLinkOpener;

        /// <summary>
        /// Handle an arbitrary URL. Displays via in-game overlays where possible.
        /// This can be called from a non-thread-safe non-game-loaded state.
        /// </summary>
        /// <param name="url">The URL to load.</param>
        public void HandleLink(string url) => HandleLink(MessageFormatter.GetLinkDetails(url));

        /// <summary>
        /// Handle a specific <see cref="LinkDetails"/>.
        /// This can be called from a non-thread-safe non-game-loaded state.
        /// </summary>
        /// <param name="link">The link to load.</param>
        public void HandleLink(LinkDetails link) => Schedule(() =>
        {
            string argString = link.Argument.ToString();

            switch (link.Action)
            {
                case LinkAction.OpenBeatmap:
                    // TODO: proper query params handling
                    if (int.TryParse(argString.Contains('?') ? argString.Split('?')[0] : argString, out int beatmapId))
                        ShowBeatmap(beatmapId);
                    break;

                case LinkAction.OpenBeatmapSet:
                    if (int.TryParse(argString, out int setId))
                        ShowBeatmapSet(setId);
                    break;

                case LinkAction.OpenChannel:
                    ShowChannel(argString);
                    break;

                case LinkAction.SearchBeatmapSet:
                    SearchBeatmapSet(argString);
                    break;

                case LinkAction.OpenEditorTimestamp:
                case LinkAction.JoinMultiplayerMatch:
                case LinkAction.Spectate:
                    waitForReady(() => Notifications, _ => Notifications.Post(new SimpleNotification
                    {
                        Text = @"This link type is not yet supported!",
                        Icon = FontAwesome.Solid.LifeRing,
                    }));
                    break;

                case LinkAction.External:
                    OpenUrlExternally(argString);
                    break;

                case LinkAction.OpenUserProfile:
                    if (!(link.Argument is IUser user))
                    {
                        user = int.TryParse(argString, out int userId)
                            ? new APIUser { Id = userId }
                            : new APIUser { Username = argString };
                    }

                    ShowUser(user);

                    break;

                case LinkAction.OpenWiki:
                    ShowWiki(argString);
                    break;

                case LinkAction.OpenChangelog:
                    if (string.IsNullOrEmpty(argString))
                        ShowChangelogListing();
                    else
                    {
                        string[] changelogArgs = argString.Split("/");
                        ShowChangelogBuild(changelogArgs[0], changelogArgs[1]);
                    }

                    break;

                default:
                    throw new NotImplementedException($"This {nameof(LinkAction)} ({link.Action.ToString()}) is missing an associated action.");
            }
        });

        public void OpenUrlExternally(string url, bool bypassExternalUrlWarning = false) => waitForReady(() => externalLinkOpener, _ =>
        {
            if (url.StartsWith('/'))
                url = $"{API.APIEndpointUrl}{url}";

            externalLinkOpener.OpenUrlExternally(url, bypassExternalUrlWarning);
        });

        /// <summary>
        /// Open a specific channel in chat.
        /// </summary>
        /// <param name="channel">The channel to display.</param>
        public void ShowChannel(string channel) => waitForReady(() => channelManager, _ =>
        {
            try
            {
                channelManager.OpenChannel(channel);
            }
            catch (ChannelNotFoundException)
            {
                Logger.Log($"The requested channel \"{channel}\" does not exist");
            }
        });

        /// <summary>
        /// Show a beatmap set as an overlay.
        /// </summary>
        /// <param name="setId">The set to display.</param>
        public void ShowBeatmapSet(int setId) => waitForReady(() => beatmapSetOverlay, _ => beatmapSetOverlay.FetchAndShowBeatmapSet(setId));

        /// <summary>
        /// Show a user's profile as an overlay.
        /// </summary>
        /// <param name="user">The user to display.</param>
        public void ShowUser(IUser user) => waitForReady(() => userProfile, _ => userProfile.ShowUser(user));

        /// <summary>
        /// Show a beatmap's set as an overlay, displaying the given beatmap.
        /// </summary>
        /// <param name="beatmapId">The beatmap to show.</param>
        public void ShowBeatmap(int beatmapId) => waitForReady(() => beatmapSetOverlay, _ => beatmapSetOverlay.FetchAndShowBeatmap(beatmapId));

        /// <summary>
        /// Shows the beatmap listing overlay, with the given <paramref name="query"/> in the search box.
        /// </summary>
        /// <param name="query">The query to search for.</param>
        public void SearchBeatmapSet(string query) => waitForReady(() => beatmapListing, _ => beatmapListing.ShowWithSearch(query));

        /// <summary>
        /// Show a wiki's page as an overlay
        /// </summary>
        /// <param name="path">The wiki page to show</param>
        public void ShowWiki(string path) => waitForReady(() => wikiOverlay, _ => wikiOverlay.ShowPage(path));

        /// <summary>
        /// Show changelog listing overlay
        /// </summary>
        public void ShowChangelogListing() => waitForReady(() => changelogOverlay, _ => changelogOverlay.ShowListing());

        /// <summary>
        /// Show changelog's build as an overlay
        /// </summary>
        /// <param name="updateStream">The update stream name</param>
        /// <param name="version">The build version of the update stream</param>
        public void ShowChangelogBuild(string updateStream, string version) => waitForReady(() => changelogOverlay, _ => changelogOverlay.ShowBuild(updateStream, version));

        /// <summary>
        /// Present a beatmap at song select immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        /// <param name="beatmap">The beatmap to select.</param>
        /// <param name="difficultyCriteria">Optional predicate used to narrow the set of difficulties to select from when presenting.</param>
        /// <remarks>
        /// Among items satisfying the predicate, the order of preference is:
        /// <list type="bullet">
        /// <item>beatmap with recommended difficulty, as provided by <see cref="DifficultyRecommender"/>,</item>
        /// <item>first beatmap from the current ruleset,</item>
        /// <item>first beatmap from any ruleset.</item>
        /// </list>
        /// </remarks>
        public void PresentBeatmap(IBeatmapSetInfo beatmap, Predicate<BeatmapInfo> difficultyCriteria = null)
        {
            Live<BeatmapSetInfo> databasedSet = null;

            if (beatmap.OnlineID > 0)
                databasedSet = BeatmapManager.QueryBeatmapSet(s => s.OnlineID == beatmap.OnlineID);

            if (beatmap is BeatmapSetInfo localBeatmap)
                databasedSet ??= BeatmapManager.QueryBeatmapSet(s => s.Hash == localBeatmap.Hash);

            if (databasedSet == null)
            {
                Logger.Log("The requested beatmap could not be loaded.", LoggingTarget.Information);
                return;
            }

            var detachedSet = databasedSet.PerformRead(s => s.Detach());

            PerformFromScreen(screen =>
            {
                // Find beatmaps that match our predicate.
                var beatmaps = detachedSet.Beatmaps.Where(b => difficultyCriteria?.Invoke(b) ?? true).ToList();

                // Use all beatmaps if predicate matched nothing
                if (beatmaps.Count == 0)
                    beatmaps = detachedSet.Beatmaps.ToList();

                // Prefer recommended beatmap if recommendations are available, else fallback to a sane selection.
                var selection = difficultyRecommender.GetRecommendedBeatmap(beatmaps)
                                ?? beatmaps.FirstOrDefault(b => b.Ruleset.Equals(Ruleset.Value))
                                ?? beatmaps.First();

                if (screen is IHandlePresentBeatmap presentableScreen)
                {
                    presentableScreen.PresentBeatmap(BeatmapManager.GetWorkingBeatmap(selection), selection.Ruleset);
                }
                else
                {
                    Ruleset.Value = selection.Ruleset;
                    Beatmap.Value = BeatmapManager.GetWorkingBeatmap(selection);
                }
            }, validScreens: new[] { typeof(SongSelect), typeof(IHandlePresentBeatmap) });
        }

        /// <summary>
        /// Present a score's replay immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        public void PresentScore(IScoreInfo score, ScorePresentType presentType = ScorePresentType.Results)
        {
            // The given ScoreInfo may have missing properties if it was retrieved from online data. Re-retrieve it from the database
            // to ensure all the required data for presenting a replay are present.
            ScoreInfo databasedScoreInfo = null;

            if (score.OnlineID > 0)
                databasedScoreInfo = ScoreManager.Query(s => s.OnlineID == score.OnlineID);

            if (score is ScoreInfo scoreInfo)
                databasedScoreInfo ??= ScoreManager.Query(s => s.Hash == scoreInfo.Hash);

            if (databasedScoreInfo == null)
            {
                Logger.Log("The requested score could not be found locally.", LoggingTarget.Information);
                return;
            }

            var databasedScore = ScoreManager.GetScore(databasedScoreInfo);

            if (databasedScore.Replay == null)
            {
                Logger.Log("The loaded score has no replay data.", LoggingTarget.Information);
                return;
            }

            var databasedBeatmap = BeatmapManager.QueryBeatmap(b => b.ID == databasedScoreInfo.BeatmapInfo.ID);

            if (databasedBeatmap == null)
            {
                Logger.Log("Tried to load a score for a beatmap we don't have!", LoggingTarget.Information);
                return;
            }

            PerformFromScreen(screen =>
            {
                Ruleset.Value = databasedScore.ScoreInfo.Ruleset;
                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(databasedBeatmap);

                switch (presentType)
                {
                    case ScorePresentType.Gameplay:
                        screen.Push(new ReplayPlayerLoader(databasedScore));
                        break;

                    case ScorePresentType.Results:
                        screen.Push(new SoloResultsScreen(databasedScore.ScoreInfo, false));
                        break;
                }
            }, validScreens: new[] { typeof(PlaySongSelect) });
        }

        public override Task Import(params ImportTask[] imports)
        {
            // encapsulate task as we don't want to begin the import process until in a ready state.

            // ReSharper disable once AsyncVoidLambda
            // TODO: This is bad because `new Task` doesn't have a Func<Task?> override.
            // Only used for android imports and a bit of a mess. Probably needs rethinking overall.
            var importTask = new Task(async () => await base.Import(imports).ConfigureAwait(false));

            waitForReady(() => this, _ => importTask.Start());

            return importTask;
        }

        protected virtual Loader CreateLoader() => new Loader();

        protected virtual UpdateManager CreateUpdateManager() => new UpdateManager();

        protected virtual HighPerformanceSession CreateHighPerformanceSession() => new HighPerformanceSession();

        protected override Container CreateScalingContainer() => new ScalingContainer(ScalingMode.Everything);

        #region Beatmap progression

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            beatmap.OldValue?.CancelAsyncLoad();
            beatmap.NewValue?.BeginAsyncLoad();
            Logger.Log($"Game-wide working beatmap updated to {beatmap.NewValue}");
        }

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            // a lease may be taken on the mods bindable, at which point we can't really ensure valid mods.
            if (SelectedMods.Disabled)
                return;

            if (!ModUtils.CheckValidForGameplay(mods.NewValue, out var invalid))
            {
                // ensure we always have a valid set of mods.
                SelectedMods.Value = mods.NewValue.Except(invalid).ToArray();
            }
        }

        #endregion

        private PerformFromMenuRunner performFromMainMenuTask;

        /// <summary>
        /// Perform an action only after returning to a specific screen as indicated by <paramref name="validScreens"/>.
        /// Eagerly tries to exit the current screen until it succeeds.
        /// </summary>
        /// <param name="action">The action to perform once we are in the correct state.</param>
        /// <param name="validScreens">An optional collection of valid screen types. If any of these screens are already current we can perform the action immediately, else the first valid parent will be made current before performing the action. <see cref="MainMenu"/> is used if not specified.</param>
        public void PerformFromScreen(Action<IScreen> action, IEnumerable<Type> validScreens = null)
        {
            performFromMainMenuTask?.Cancel();
            Add(performFromMainMenuTask = new PerformFromMenuRunner(action, validScreens, () => ScreenStack.CurrentScreen));
        }

        /// <summary>
        /// Wait for the game (and target component) to become loaded and then run an action.
        /// </summary>
        /// <param name="retrieveInstance">A function to retrieve a (potentially not-yet-constructed) target instance.</param>
        /// <param name="action">The action to perform on the instance when load is confirmed.</param>
        /// <typeparam name="T">The type of the target instance.</typeparam>
        private void waitForReady<T>(Func<T> retrieveInstance, Action<T> action)
            where T : Drawable
        {
            var instance = retrieveInstance();

            if (ScreenStack == null || ScreenStack.CurrentScreen is StartupScreen || instance?.IsLoaded != true)
                Schedule(() => waitForReady(retrieveInstance, action));
            else
                action(instance);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            SentryLogger.Dispose();
        }

        protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults()
            => new Dictionary<FrameworkSetting, object>
            {
                // General expectation that osu! starts in fullscreen by default (also gives the most predictable performance)
                { FrameworkSetting.WindowMode, WindowMode.Fullscreen }
            };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var language in Enum.GetValues(typeof(Language)).OfType<Language>())
            {
                string cultureCode = language.ToCultureCode();

                try
                {
                    Localisation.AddLanguage(cultureCode, new ResourceManagerLocalisationStore(cultureCode));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Could not load localisations for language \"{cultureCode}\"");
                }
            }

            // The next time this is updated is in UpdateAfterChildren, which occurs too late and results
            // in the cursor being shown for a few frames during the intro.
            // This prevents the cursor from showing until we have a screen with CursorVisible = true
            MenuCursorContainer.CanShowCursor = menuScreen?.CursorVisible ?? false;

            // todo: all archive managers should be able to be looped here.
            SkinManager.PostNotification = n => Notifications.Post(n);

            BeatmapManager.PostNotification = n => Notifications.Post(n);
            BeatmapManager.PostImport = items => PresentBeatmap(items.First().Value);

            BeatmapDownloader.PostNotification = n => Notifications.Post(n);
            ScoreDownloader.PostNotification = n => Notifications.Post(n);

            ScoreManager.PostNotification = n => Notifications.Post(n);
            ScoreManager.PostImport = items => PresentScore(items.First().Value);

            // make config aware of how to lookup skins for on-screen display purposes.
            // if this becomes a more common thing, tracked settings should be reconsidered to allow local DI.
            LocalConfig.LookupSkinName = id => SkinManager.Query(s => s.ID == id)?.ToString() ?? "Unknown";

            LocalConfig.LookupKeyBindings = l =>
            {
                var combinations = KeyBindingStore.GetReadableKeyCombinationsFor(l);

                if (combinations.Count == 0)
                    return ToastStrings.NoKeyBound;

                return string.Join(" / ", combinations);
            };

            Container logoContainer;
            BackButton.Receptor receptor;

            dependencies.CacheAs(idleTracker = new GameIdleTracker(6000));

            var sessionIdleTracker = new GameIdleTracker(300000);
            sessionIdleTracker.IsIdle.BindValueChanged(idle =>
            {
                if (idle.NewValue)
                    SessionStatics.ResetAfterInactivity();
            });

            Add(sessionIdleTracker);

            AddRange(new Drawable[]
            {
                new VolumeControlReceptor
                {
                    RelativeSizeAxes = Axes.Both,
                    ActionRequested = action => volume.Adjust(action),
                    ScrollActionRequested = (action, amount, isPrecise) => volume.Adjust(action, amount, isPrecise),
                },
                ScreenOffsetContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        ScreenContainer = new ScalingContainer(ScalingMode.ExcludeOverlays)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                receptor = new BackButton.Receptor(),
                                ScreenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both },
                                BackButton = new BackButton(receptor)
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Action = () =>
                                    {
                                        if (!(ScreenStack.CurrentScreen is IOsuScreen currentScreen))
                                            return;

                                        if (!((Drawable)currentScreen).IsLoaded || (currentScreen.AllowBackButton && !currentScreen.OnBackButton()))
                                            ScreenStack.Exit();
                                    }
                                },
                                logoContainer = new Container { RelativeSizeAxes = Axes.Both },
                            }
                        },
                    }
                },
                overlayOffsetContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        overlayContent = new Container { RelativeSizeAxes = Axes.Both },
                        rightFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                        leftFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                    }
                },
                topMostOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                idleTracker,
                new ConfineMouseTracker()
            });

            ScreenStack.ScreenPushed += screenPushed;
            ScreenStack.ScreenExited += screenExited;

            if (!args?.Any(a => a == @"--no-version-overlay") ?? true)
                loadComponentSingleFile(versionManager = new VersionManager { Depth = int.MinValue }, ScreenContainer.Add);

            loadComponentSingleFile(osuLogo, logo =>
            {
                logoContainer.Add(logo);

                // Loader has to be created after the logo has finished loading as Loader performs logo transformations on entering.
                ScreenStack.Push(CreateLoader().With(l => l.RelativeSizeAxes = Axes.Both));
            });

            loadComponentSingleFile(Toolbar = new Toolbar
            {
                OnHome = delegate
                {
                    CloseAllOverlays(false);
                    menuScreen?.MakeCurrent();
                },
            }, topMostOverlayContent.Add);

            loadComponentSingleFile(volume = new VolumeOverlay(), leftFloatingOverlayContent.Add, true);

            var onScreenDisplay = new OnScreenDisplay();

            onScreenDisplay.BeginTracking(this, frameworkConfig);
            onScreenDisplay.BeginTracking(this, LocalConfig);

            loadComponentSingleFile(onScreenDisplay, Add, true);

            loadComponentSingleFile(Notifications.With(d =>
            {
                d.Anchor = Anchor.TopRight;
                d.Origin = Anchor.TopRight;
            }), rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new CollectionManager(Storage)
            {
                PostNotification = n => Notifications.Post(n),
            }, Add, true);

            loadComponentSingleFile(legacyImportManager, Add);

            loadComponentSingleFile(screenshotManager, Add);

            // dependency on notification overlay, dependent by settings overlay
            loadComponentSingleFile(CreateUpdateManager(), Add, true);

            // overlay elements
            loadComponentSingleFile(new ManageCollectionsDialog(), overlayContent.Add, true);
            loadComponentSingleFile(beatmapListing = new BeatmapListingOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(dashboard = new DashboardOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(news = new NewsOverlay(), overlayContent.Add, true);
            var rankingsOverlay = loadComponentSingleFile(new RankingsOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(channelManager = new ChannelManager(), AddInternal, true);
            loadComponentSingleFile(chatOverlay = new ChatOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(new MessageNotifier(), AddInternal, true);
            loadComponentSingleFile(Settings = new SettingsOverlay(), leftFloatingOverlayContent.Add, true);
            loadComponentSingleFile(changelogOverlay = new ChangelogOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(userProfile = new UserProfileOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(beatmapSetOverlay = new BeatmapSetOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(wikiOverlay = new WikiOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(skinEditor = new SkinEditorOverlay(ScreenContainer), overlayContent.Add, true);

            loadComponentSingleFile(new LoginOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new NowPlayingOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new AccountCreationOverlay(), topMostOverlayContent.Add, true);
            loadComponentSingleFile(new DialogOverlay(), topMostOverlayContent.Add, true);

            loadComponentSingleFile(CreateHighPerformanceSession(), Add);

            chatOverlay.State.BindValueChanged(_ => updateChatPollRate());
            // Multiplayer modes need to increase poll rate temporarily.
            API.Activity.BindValueChanged(_ => updateChatPollRate(), true);

            void updateChatPollRate()
            {
                channelManager.HighPollRate.Value =
                    chatOverlay.State.Value == Visibility.Visible
                    || API.Activity.Value is UserActivity.InLobby
                    || API.Activity.Value is UserActivity.InMultiplayerGame;
            }

            Add(difficultyRecommender);
            Add(externalLinkOpener = new ExternalLinkOpener());
            Add(new MusicKeyBindingHandler());

            // side overlays which cancel each other.
            var singleDisplaySideOverlays = new OverlayContainer[] { Settings, Notifications };

            foreach (var overlay in singleDisplaySideOverlays)
            {
                overlay.State.ValueChanged += state =>
                {
                    if (state.NewValue == Visibility.Hidden) return;

                    singleDisplaySideOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            // eventually informational overlays should be displayed in a stack, but for now let's only allow one to stay open at a time.
            var informationalOverlays = new OverlayContainer[] { beatmapSetOverlay, userProfile };

            foreach (var overlay in informationalOverlays)
            {
                overlay.State.ValueChanged += state =>
                {
                    if (state.NewValue != Visibility.Hidden)
                        showOverlayAboveOthers(overlay, informationalOverlays);
                };
            }

            // ensure only one of these overlays are open at once.
            var singleDisplayOverlays = new OverlayContainer[] { chatOverlay, news, dashboard, beatmapListing, changelogOverlay, rankingsOverlay, wikiOverlay };

            foreach (var overlay in singleDisplayOverlays)
            {
                overlay.State.ValueChanged += state =>
                {
                    // informational overlays should be dismissed on a show or hide of a full overlay.
                    informationalOverlays.ForEach(o => o.Hide());

                    if (state.NewValue != Visibility.Hidden)
                        showOverlayAboveOthers(overlay, singleDisplayOverlays);
                };
            }

            OverlayActivationMode.ValueChanged += mode =>
            {
                if (mode.NewValue != OverlayActivation.All) CloseAllOverlays();
            };

            // Importantly, this should be run after binding PostNotification to the import handlers so they can present the import after game startup.
            handleStartupImport();
        }

        private void handleStartupImport()
        {
            if (args?.Length > 0)
            {
                string[] paths = args.Where(a => !a.StartsWith('-')).ToArray();
                if (paths.Length > 0)
                    Task.Run(() => Import(paths));
            }
        }

        private void showOverlayAboveOthers(OverlayContainer overlay, OverlayContainer[] otherOverlays)
        {
            otherOverlays.Where(o => o != overlay).ForEach(o => o.Hide());

            // Partially visible so leave it at the current depth.
            if (overlay.IsPresent)
                return;

            // Show above all other overlays.
            if (overlay.IsLoaded)
                overlayContent.ChangeChildDepth(overlay, (float)-Clock.CurrentTime);
            else
                overlay.Depth = (float)-Clock.CurrentTime;
        }

        private void forwardLoggedErrorsToNotifications()
        {
            int recentLogCount = 0;

            const double debounce = 60000;

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important || entry.Target == null) return;

                const int short_term_display_limit = 3;

                if (recentLogCount < short_term_display_limit)
                {
                    Schedule(() => Notifications.Post(new SimpleErrorNotification
                    {
                        Icon = entry.Level == LogLevel.Important ? FontAwesome.Solid.ExclamationCircle : FontAwesome.Solid.Bomb,
                        Text = entry.Message.Truncate(256) + (entry.Exception != null && IsDeployedBuild ? "\n\nThis error has been automatically reported to the devs." : string.Empty),
                    }));
                }
                else if (recentLogCount == short_term_display_limit)
                {
                    string logFile = $@"{entry.Target.ToString().ToLowerInvariant()}.log";

                    Schedule(() => Notifications.Post(new SimpleNotification
                    {
                        Icon = FontAwesome.Solid.EllipsisH,
                        Text = "Subsequent messages have been logged. Click to view log files.",
                        Activated = () =>
                        {
                            Storage.GetStorageForDirectory(@"logs").PresentFileExternally(logFile);
                            return true;
                        }
                    }));
                }

                Interlocked.Increment(ref recentLogCount);
                Scheduler.AddDelayed(() => Interlocked.Decrement(ref recentLogCount), debounce);
            };
        }

        private Task asyncLoadStream;

        /// <summary>
        /// Queues loading the provided component in sequential fashion.
        /// This operation is limited to a single thread to avoid saturating all cores.
        /// </summary>
        /// <param name="component">The component to load.</param>
        /// <param name="loadCompleteAction">An action to invoke on load completion (generally to add the component to the hierarchy).</param>
        /// <param name="cache">Whether to cache the component as type <typeparamref name="T"/> into the game dependencies before any scheduling.</param>
        private T loadComponentSingleFile<T>(T component, Action<T> loadCompleteAction, bool cache = false)
            where T : Drawable
        {
            if (cache)
                dependencies.CacheAs(component);

            if (component is OsuFocusedOverlayContainer overlay)
                focusedOverlays.Add(overlay);

            // schedule is here to ensure that all component loads are done after LoadComplete is run (and thus all dependencies are cached).
            // with some better organisation of LoadComplete to do construction and dependency caching in one step, followed by calls to loadComponentSingleFile,
            // we could avoid the need for scheduling altogether.
            Schedule(() =>
            {
                var previousLoadStream = asyncLoadStream;

                // chain with existing load stream
                asyncLoadStream = Task.Run(async () =>
                {
                    if (previousLoadStream != null)
                        await previousLoadStream.ConfigureAwait(false);

                    try
                    {
                        Logger.Log($"Loading {component}...");

                        // Since this is running in a separate thread, it is possible for OsuGame to be disposed after LoadComponentAsync has been called
                        // throwing an exception. To avoid this, the call is scheduled on the update thread, which does not run if IsDisposed = true
                        Task task = null;
                        var del = new ScheduledDelegate(() => task = LoadComponentAsync(component, loadCompleteAction));
                        Scheduler.Add(del);

                        // The delegate won't complete if OsuGame has been disposed in the meantime
                        while (!IsDisposed && !del.Completed)
                            await Task.Delay(10).ConfigureAwait(false);

                        // Either we're disposed or the load process has started successfully
                        if (IsDisposed)
                            return;

                        Debug.Assert(task != null);

                        await task.ConfigureAwait(false);

                        Logger.Log($"Loaded {component}!");
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });
            });

            return component;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (introScreen == null) return false;

            switch (e.Action)
            {
                case GlobalAction.ResetInputSettings:
                    Host.ResetInputHandlers();
                    frameworkConfig.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode).SetDefault();
                    return true;

                case GlobalAction.ToggleGameplayMouseButtons:
                    var mouseDisableButtons = LocalConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons);
                    mouseDisableButtons.Value = !mouseDisableButtons.Value;
                    return true;

                case GlobalAction.RandomSkin:
                    SkinManager.SelectRandomSkin();
                    return true;
            }

            return false;
        }

        public override bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            const float adjustment_increment = 0.05f;

            switch (e.Action)
            {
                case PlatformAction.ZoomIn:
                    uiScale.Value += adjustment_increment;
                    return true;

                case PlatformAction.ZoomOut:
                    uiScale.Value -= adjustment_increment;
                    return true;

                case PlatformAction.ZoomDefault:
                    uiScale.SetDefault();
                    return true;
            }

            return base.OnPressed(e);
        }

        #region Inactive audio dimming

        private readonly BindableDouble inactiveVolumeFade = new BindableDouble();

        private void updateActiveState(bool isActive)
        {
            if (isActive)
                this.TransformBindableTo(inactiveVolumeFade, 1, 400, Easing.OutQuint);
            else
                this.TransformBindableTo(inactiveVolumeFade, LocalConfig.Get<double>(OsuSetting.VolumeInactive), 4000, Easing.OutQuint);
        }

        #endregion

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override bool OnExiting()
        {
            if (ScreenStack.CurrentScreen is Loader)
                return false;

            if (introScreen?.DidLoadMenu == true && !(ScreenStack.CurrentScreen is IntroScreen))
            {
                Scheduler.Add(introScreen.MakeCurrent);
                return true;
            }

            return base.OnExiting();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            ScreenOffsetContainer.Padding = new MarginPadding { Top = toolbarOffset };
            overlayOffsetContainer.Padding = new MarginPadding { Top = toolbarOffset };

            float horizontalOffset = 0f;

            // Content.ToLocalSpace() is used instead of this.ToLocalSpace() to correctly calculate the offset with scaling modes active.
            // Content is a child of a scaling container with ScalingMode.Everything set, while the game itself is never scaled.
            // this avoids a visible jump in the positioning of the screen offset container.
            if (Settings.IsLoaded && Settings.IsPresent)
                horizontalOffset += Content.ToLocalSpace(Settings.ScreenSpaceDrawQuad.TopRight).X * SIDE_OVERLAY_OFFSET_RATIO;
            if (Notifications.IsLoaded && Notifications.IsPresent)
                horizontalOffset += (Content.ToLocalSpace(Notifications.ScreenSpaceDrawQuad.TopLeft).X - Content.DrawWidth) * SIDE_OVERLAY_OFFSET_RATIO;

            ScreenOffsetContainer.X = horizontalOffset;

            MenuCursorContainer.CanShowCursor = (ScreenStack.CurrentScreen as IOsuScreen)?.CursorVisible ?? false;
        }

        protected virtual void ScreenChanged(IScreen current, IScreen newScreen)
        {
            skinEditor.Reset();

            switch (newScreen)
            {
                case IntroScreen intro:
                    introScreen = intro;
                    versionManager?.Show();
                    break;

                case MainMenu menu:
                    menuScreen = menu;
                    versionManager?.Show();
                    break;

                default:
                    versionManager?.Hide();
                    break;
            }

            // reset on screen change for sanity.
            LocalUserPlaying.Value = false;

            if (current is IOsuScreen currentOsuScreen)
            {
                OverlayActivationMode.UnbindFrom(currentOsuScreen.OverlayActivationMode);
                API.Activity.UnbindFrom(currentOsuScreen.Activity);
            }

            if (newScreen is IOsuScreen newOsuScreen)
            {
                OverlayActivationMode.BindTo(newOsuScreen.OverlayActivationMode);
                API.Activity.BindTo(newOsuScreen.Activity);

                if (newOsuScreen.HideOverlaysOnEnter)
                    CloseAllOverlays();
                else
                    Toolbar.Show();

                if (newOsuScreen.AllowBackButton)
                    BackButton.Show();
                else
                    BackButton.Hide();
            }
        }

        private void screenPushed(IScreen lastScreen, IScreen newScreen) => ScreenChanged(lastScreen, newScreen);

        private void screenExited(IScreen lastScreen, IScreen newScreen)
        {
            ScreenChanged(lastScreen, newScreen);

            if (newScreen == null)
                Exit();
        }

        IBindable<bool> ILocalUserPlayInfo.IsPlaying => LocalUserPlaying;
    }
}
