// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Framework.Logging;
using osu.Framework.Allocation;
using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Overlays.Notifications;
using osu.Game.Input.Bindings;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Music;
using osu.Game.Skinning;
using osuTK.Graphics;
using osu.Game.Overlays.Volume;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Updater;
using osu.Game.Utils;
using LogLevel = osu.Framework.Logging.LogLevel;
using System.IO;

namespace osu.Game
{
    /// <summary>
    /// The full osu! experience. Builds on top of <see cref="OsuGameBase"/> to add menus and binding logic
    /// for initial components that are generally retrieved via DI.
    /// </summary>
    public class OsuGame : OsuGameBase, IKeyBindingHandler<GlobalAction>
    {
        public Toolbar Toolbar;

        private ChatOverlay chatOverlay;

        private ChannelManager channelManager;

        [NotNull]
        private readonly NotificationOverlay notifications = new NotificationOverlay();

        private BeatmapListingOverlay beatmapListing;

        private DashboardOverlay dashboard;

        private NewsOverlay news;

        private UserProfileOverlay userProfile;

        private BeatmapSetOverlay beatmapSetOverlay;

        [Cached]
        private readonly DifficultyRecommender difficultyRecommender = new DifficultyRecommender();

        [Cached]
        private readonly ScreenshotManager screenshotManager = new ScreenshotManager();

        protected SentryLogger SentryLogger;

        public virtual Storage GetStorageForStableInstall() => null;

        public float ToolbarOffset => (Toolbar?.Position.Y ?? 0) + (Toolbar?.DrawHeight ?? 0);

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

        [CanBeNull]
        private IntroScreen introScreen;

        private Bindable<int> configRuleset;

        private Bindable<int> configSkin;

        private readonly string[] args;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        private readonly List<OverlayContainer> visibleBlockingOverlays = new List<OverlayContainer>();

        public OsuGame(string[] args = null)
        {
            this.args = args;

            forwardLoggedErrorsToNotifications();

            SentryLogger = new SentryLogger(this);
        }

        private void updateBlockingOverlayFade() =>
            screenContainer.FadeColour(visibleBlockingOverlays.Any() ? OsuColour.Gray(0.5f) : Color4.White, 500, Easing.OutQuint);

        public void AddBlockingOverlay(OverlayContainer overlay)
        {
            if (!visibleBlockingOverlays.Contains(overlay))
                visibleBlockingOverlays.Add(overlay);
            updateBlockingOverlayFade();
        }

        public void RemoveBlockingOverlay(OverlayContainer overlay)
        {
            visibleBlockingOverlays.Remove(overlay);
            updateBlockingOverlayFade();
        }

        /// <summary>
        /// Close all game-wide overlays.
        /// </summary>
        /// <param name="hideToolbar">Whether the toolbar should also be hidden.</param>
        public void CloseAllOverlays(bool hideToolbar = true)
        {
            foreach (var overlay in overlays)
                overlay.Hide();

            if (hideToolbar) Toolbar.Hide();
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!Host.IsPrimaryInstance && !DebugUtils.IsDebugBuild)
            {
                Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                Environment.Exit(0);
            }

            if (args?.Length > 0)
            {
                var paths = args.Where(a => !a.StartsWith('-')).ToArray();
                if (paths.Length > 0)
                    Task.Run(() => Import(paths));
            }

            dependencies.CacheAs(this);

            dependencies.Cache(SentryLogger);

            dependencies.Cache(osuLogo = new OsuLogo { Alpha = 0 });

            // bind config int to database RulesetInfo
            configRuleset = LocalConfig.GetBindable<int>(OsuSetting.Ruleset);
            Ruleset.Value = RulesetStore.GetRuleset(configRuleset.Value) ?? RulesetStore.AvailableRulesets.First();
            Ruleset.ValueChanged += r => configRuleset.Value = r.NewValue.ID ?? 0;

            // bind config int to database SkinInfo
            configSkin = LocalConfig.GetBindable<int>(OsuSetting.Skin);
            SkinManager.CurrentSkinInfo.ValueChanged += skin => configSkin.Value = skin.NewValue.ID;
            configSkin.ValueChanged += skinId =>
            {
                var skinInfo = SkinManager.Query(s => s.ID == skinId.NewValue);

                if (skinInfo == null)
                {
                    switch (skinId.NewValue)
                    {
                        case -1:
                            skinInfo = DefaultLegacySkin.Info;
                            break;

                        default:
                            skinInfo = SkinInfo.Default;
                            break;
                    }
                }

                SkinManager.CurrentSkinInfo.Value = skinInfo;
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
            switch (link.Action)
            {
                case LinkAction.OpenBeatmap:
                    // TODO: proper query params handling
                    if (link.Argument != null && int.TryParse(link.Argument.Contains('?') ? link.Argument.Split('?')[0] : link.Argument, out int beatmapId))
                        ShowBeatmap(beatmapId);
                    break;

                case LinkAction.OpenBeatmapSet:
                    if (int.TryParse(link.Argument, out int setId))
                        ShowBeatmapSet(setId);
                    break;

                case LinkAction.OpenChannel:
                    ShowChannel(link.Argument);
                    break;

                case LinkAction.OpenEditorTimestamp:
                case LinkAction.JoinMultiplayerMatch:
                case LinkAction.Spectate:
                    waitForReady(() => notifications, _ => notifications.Post(new SimpleNotification
                    {
                        Text = @"This link type is not yet supported!",
                        Icon = FontAwesome.Solid.LifeRing,
                    }));
                    break;

                case LinkAction.External:
                    OpenUrlExternally(link.Argument);
                    break;

                case LinkAction.OpenUserProfile:
                    if (int.TryParse(link.Argument, out int userId))
                        ShowUser(userId);
                    break;

                default:
                    throw new NotImplementedException($"This {nameof(LinkAction)} ({link.Action.ToString()}) is missing an associated action.");
            }
        });

        public void OpenUrlExternally(string url) => waitForReady(() => externalLinkOpener, _ =>
        {
            if (url.StartsWith('/'))
                url = $"{API.APIEndpointUrl}{url}";

            externalLinkOpener.OpenUrlExternally(url);
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
        /// <param name="userId">The user to display.</param>
        public void ShowUser(int userId) => waitForReady(() => userProfile, _ => userProfile.ShowUser(userId));

        /// <summary>
        /// Show a beatmap's set as an overlay, displaying the given beatmap.
        /// </summary>
        /// <param name="beatmapId">The beatmap to show.</param>
        public void ShowBeatmap(int beatmapId) => waitForReady(() => beatmapSetOverlay, _ => beatmapSetOverlay.FetchAndShowBeatmap(beatmapId));

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
        public void PresentBeatmap(BeatmapSetInfo beatmap, Predicate<BeatmapInfo> difficultyCriteria = null)
        {
            var databasedSet = beatmap.OnlineBeatmapSetID != null
                ? BeatmapManager.QueryBeatmapSet(s => s.OnlineBeatmapSetID == beatmap.OnlineBeatmapSetID)
                : BeatmapManager.QueryBeatmapSet(s => s.Hash == beatmap.Hash);

            if (databasedSet == null)
            {
                Logger.Log("The requested beatmap could not be loaded.", LoggingTarget.Information);
                return;
            }

            PerformFromScreen(screen =>
            {
                // we might already be at song select, so a check is required before performing the load to solo.
                if (screen is MainMenu)
                    menuScreen.LoadToSolo();

                // we might even already be at the song
                if (Beatmap.Value.BeatmapSetInfo.Hash == databasedSet.Hash && (difficultyCriteria?.Invoke(Beatmap.Value.BeatmapInfo) ?? true))
                    return;

                // Find beatmaps that match our predicate.
                var beatmaps = databasedSet.Beatmaps.Where(b => difficultyCriteria?.Invoke(b) ?? true).ToList();

                // Use all beatmaps if predicate matched nothing
                if (beatmaps.Count == 0)
                    beatmaps = databasedSet.Beatmaps;

                // Prefer recommended beatmap if recommendations are available, else fallback to a sane selection.
                var selection = difficultyRecommender.GetRecommendedBeatmap(beatmaps)
                                ?? beatmaps.FirstOrDefault(b => b.Ruleset.Equals(Ruleset.Value))
                                ?? beatmaps.First();

                Ruleset.Value = selection.Ruleset;
                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(selection);
            }, validScreens: new[] { typeof(PlaySongSelect) });
        }

        /// <summary>
        /// Present a score's replay immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        public void PresentScore(ScoreInfo score, ScorePresentType presentType = ScorePresentType.Results)
        {
            // The given ScoreInfo may have missing properties if it was retrieved from online data. Re-retrieve it from the database
            // to ensure all the required data for presenting a replay are present.
            var databasedScoreInfo = score.OnlineScoreID != null
                ? ScoreManager.Query(s => s.OnlineScoreID == score.OnlineScoreID)
                : ScoreManager.Query(s => s.Hash == score.Hash);

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

            var databasedBeatmap = BeatmapManager.QueryBeatmap(b => b.ID == databasedScoreInfo.Beatmap.ID);

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

        public override Task Import(Stream stream, string filename)
        {
            // encapsulate task as we don't want to begin the import process until in a ready state.
            var importTask = new Task(async () => await base.Import(stream, filename));

            waitForReady(() => this, _ => importTask.Start());

            return importTask;
        }

        protected virtual Loader CreateLoader() => new Loader();

        protected virtual UpdateManager CreateUpdateManager() => new UpdateManager();

        protected override Container CreateScalingContainer() => new ScalingContainer(ScalingMode.Everything);

        #region Beatmap progression

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            beatmap.OldValue?.CancelAsyncLoad();

            updateModDefaults();

            beatmap.NewValue?.BeginAsyncLoad();
        }

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            updateModDefaults();
        }

        private void updateModDefaults()
        {
            BeatmapDifficulty baseDifficulty = Beatmap.Value.BeatmapInfo.BaseDifficulty;

            if (baseDifficulty != null && SelectedMods.Value.Any(m => m is IApplicableToDifficulty))
            {
                var adjustedDifficulty = baseDifficulty.Clone();

                foreach (var mod in SelectedMods.Value.OfType<IApplicableToDifficulty>())
                    mod.ReadFromDifficulty(adjustedDifficulty);
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // The next time this is updated is in UpdateAfterChildren, which occurs too late and results
            // in the cursor being shown for a few frames during the intro.
            // This prevents the cursor from showing until we have a screen with CursorVisible = true
            MenuCursorContainer.CanShowCursor = menuScreen?.CursorVisible ?? false;

            // todo: all archive managers should be able to be looped here.
            SkinManager.PostNotification = n => notifications.Post(n);
            SkinManager.GetStableStorage = GetStorageForStableInstall;

            BeatmapManager.PostNotification = n => notifications.Post(n);
            BeatmapManager.GetStableStorage = GetStorageForStableInstall;
            BeatmapManager.PresentImport = items => PresentBeatmap(items.First());

            ScoreManager.PostNotification = n => notifications.Post(n);
            ScoreManager.GetStableStorage = GetStorageForStableInstall;
            ScoreManager.PresentImport = items => PresentScore(items.First());

            // make config aware of how to lookup skins for on-screen display purposes.
            // if this becomes a more common thing, tracked settings should be reconsidered to allow local DI.
            LocalConfig.LookupSkinName = id => SkinManager.GetAllUsableSkins().FirstOrDefault(s => s.ID == id)?.ToString() ?? "Unknown";

            LocalConfig.LookupKeyBindings = l =>
            {
                var combinations = KeyBindingStore.GetReadableKeyCombinationsFor(l).ToArray();

                if (combinations.Length == 0)
                    return "none";

                return string.Join(" or ", combinations);
            };

            Container logoContainer;
            BackButton.Receptor receptor;

            dependencies.CacheAs(idleTracker = new GameIdleTracker(6000));

            AddRange(new Drawable[]
            {
                new VolumeControlReceptor
                {
                    RelativeSizeAxes = Axes.Both,
                    ActionRequested = action => volume.Adjust(action),
                    ScrollActionRequested = (action, amount, isPrecise) => volume.Adjust(action, amount, isPrecise),
                },
                screenContainer = new ScalingContainer(ScalingMode.ExcludeOverlays)
                {
                    RelativeSizeAxes = Axes.Both,
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
                                var currentScreen = ScreenStack.CurrentScreen as IOsuScreen;

                                if (currentScreen?.AllowBackButton == true && !currentScreen.OnBackButton())
                                    ScreenStack.Exit();
                            }
                        },
                        logoContainer = new Container { RelativeSizeAxes = Axes.Both },
                    }
                },
                overlayContent = new Container { RelativeSizeAxes = Axes.Both },
                rightFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                leftFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                topMostOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                idleTracker,
                new ConfineMouseTracker()
            });

            ScreenStack.ScreenPushed += screenPushed;
            ScreenStack.ScreenExited += screenExited;

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

            loadComponentSingleFile(notifications.With(d =>
            {
                d.GetToolbarHeight = () => ToolbarOffset;
                d.Anchor = Anchor.TopRight;
                d.Origin = Anchor.TopRight;
            }), rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new CollectionManager(Storage)
            {
                PostNotification = n => notifications.Post(n),
                GetStableStorage = GetStorageForStableInstall
            }, Add, true);

            loadComponentSingleFile(difficultyRecommender, Add);

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
            loadComponentSingleFile(Settings = new SettingsOverlay { GetToolbarHeight = () => ToolbarOffset }, leftFloatingOverlayContent.Add, true);
            var changelogOverlay = loadComponentSingleFile(new ChangelogOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(userProfile = new UserProfileOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(beatmapSetOverlay = new BeatmapSetOverlay(), overlayContent.Add, true);

            loadComponentSingleFile(new LoginOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new NowPlayingOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new AccountCreationOverlay(), topMostOverlayContent.Add, true);
            loadComponentSingleFile(new DialogOverlay(), topMostOverlayContent.Add, true);

            chatOverlay.State.ValueChanged += state => channelManager.HighPollRate.Value = state.NewValue == Visibility.Visible;

            Add(externalLinkOpener = new ExternalLinkOpener());
            Add(new MusicKeyBindingHandler());

            // side overlays which cancel each other.
            var singleDisplaySideOverlays = new OverlayContainer[] { Settings, notifications };

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
            var singleDisplayOverlays = new OverlayContainer[] { chatOverlay, news, dashboard, beatmapListing, changelogOverlay, rankingsOverlay };

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

            void updateScreenOffset()
            {
                float offset = 0;

                if (Settings.State.Value == Visibility.Visible)
                    offset += Toolbar.HEIGHT / 2;
                if (notifications.State.Value == Visibility.Visible)
                    offset -= Toolbar.HEIGHT / 2;

                screenContainer.MoveToX(offset, SettingsPanel.TRANSITION_LENGTH, Easing.OutQuint);
            }

            Settings.State.ValueChanged += _ => updateScreenOffset();
            notifications.State.ValueChanged += _ => updateScreenOffset();
        }

        private void showOverlayAboveOthers(OverlayContainer overlay, OverlayContainer[] otherOverlays)
        {
            otherOverlays.Where(o => o != overlay).ForEach(o => o.Hide());

            // show above others if not visible at all, else leave at current depth.
            if (!overlay.IsPresent)
                overlayContent.ChangeChildDepth(overlay, (float)-Clock.CurrentTime);
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
                    Schedule(() => notifications.Post(new SimpleNotification
                    {
                        Icon = entry.Level == LogLevel.Important ? FontAwesome.Solid.ExclamationCircle : FontAwesome.Solid.Bomb,
                        Text = entry.Message.Truncate(256) + (entry.Exception != null && IsDeployedBuild ? "\n\nThis error has been automatically reported to the devs." : string.Empty),
                    }));
                }
                else if (recentLogCount == short_term_display_limit)
                {
                    Schedule(() => notifications.Post(new SimpleNotification
                    {
                        Icon = FontAwesome.Solid.EllipsisH,
                        Text = "Subsequent messages have been logged. Click to view log files.",
                        Activated = () =>
                        {
                            Storage.GetStorageForDirectory("logs").OpenInNativeExplorer();
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

            if (component is OverlayContainer overlay)
                overlays.Add(overlay);

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
                        await previousLoadStream;

                    try
                    {
                        Logger.Log($"Loading {component}...", level: LogLevel.Debug);

                        // Since this is running in a separate thread, it is possible for OsuGame to be disposed after LoadComponentAsync has been called
                        // throwing an exception. To avoid this, the call is scheduled on the update thread, which does not run if IsDisposed = true
                        Task task = null;
                        var del = new ScheduledDelegate(() => task = LoadComponentAsync(component, loadCompleteAction));
                        Scheduler.Add(del);

                        // The delegate won't complete if OsuGame has been disposed in the meantime
                        while (!IsDisposed && !del.Completed)
                            await Task.Delay(10);

                        // Either we're disposed or the load process has started successfully
                        if (IsDisposed)
                            return;

                        Debug.Assert(task != null);

                        await task;

                        Logger.Log($"Loaded {component}!", level: LogLevel.Debug);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });
            });

            return component;
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // forward any unhandled mouse scroll events to the volume control.
            volume.Adjust(GlobalAction.IncreaseVolume, e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }

        public bool OnPressed(GlobalAction action)
        {
            if (introScreen == null) return false;

            switch (action)
            {
                case GlobalAction.ResetInputSettings:
                    var sensitivity = frameworkConfig.GetBindable<double>(FrameworkSetting.CursorSensitivity);

                    sensitivity.Disabled = false;
                    sensitivity.Value = 1;
                    sensitivity.Disabled = true;

                    frameworkConfig.Set(FrameworkSetting.IgnoredInputHandlers, string.Empty);
                    frameworkConfig.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode).SetDefault();
                    return true;

                case GlobalAction.ToggleToolbar:
                    Toolbar.ToggleVisibility();
                    return true;

                case GlobalAction.ToggleGameplayMouseButtons:
                    LocalConfig.Set(OsuSetting.MouseDisableButtons, !LocalConfig.Get<bool>(OsuSetting.MouseDisableButtons));
                    return true;

                case GlobalAction.RandomSkin:
                    SkinManager.SelectRandomSkin();
                    return true;
            }

            return false;
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

        public void OnReleased(GlobalAction action)
        {
        }

        private Container overlayContent;

        private Container rightFloatingOverlayContent;

        private Container leftFloatingOverlayContent;

        private Container topMostOverlayContent;

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; }

        private ScalingContainer screenContainer;

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

        /// <summary>
        /// Use to programatically exit the game as if the user was triggering via alt-f4.
        /// Will keep persisting until an exit occurs (exit may be blocked multiple times).
        /// </summary>
        public void GracefullyExit()
        {
            if (!OnExiting())
                Exit();
            else
                Scheduler.AddDelayed(GracefullyExit, 2000);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            screenContainer.Padding = new MarginPadding { Top = ToolbarOffset };
            overlayContent.Padding = new MarginPadding { Top = ToolbarOffset };

            MenuCursorContainer.CanShowCursor = (ScreenStack.CurrentScreen as IOsuScreen)?.CursorVisible ?? false;
        }

        protected virtual void ScreenChanged(IScreen current, IScreen newScreen)
        {
            switch (newScreen)
            {
                case IntroScreen intro:
                    introScreen = intro;
                    break;

                case MainMenu menu:
                    menuScreen = menu;
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

                MusicController.AllowRateAdjustments = newOsuScreen.AllowRateAdjustments;

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

        private void screenPushed(IScreen lastScreen, IScreen newScreen)
        {
            ScreenChanged(lastScreen, newScreen);
            Logger.Log($"Screen changed → {newScreen}");
        }

        private void screenExited(IScreen lastScreen, IScreen newScreen)
        {
            ScreenChanged(lastScreen, newScreen);
            Logger.Log($"Screen changed ← {newScreen}");

            if (newScreen == null)
                Exit();
        }
    }
}
