// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
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
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.Music;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Overlays.Toolbar;
using osu.Game.Overlays.Volume;
using osu.Game.Performance;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Skinning;
using osu.Game.Updater;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK.Graphics;
using Sentry;

namespace osu.Game
{
    /// <summary>
    /// The full osu! experience. Builds on top of <see cref="OsuGameBase"/> to add menus and binding logic
    /// for initial components that are generally retrieved via DI.
    /// </summary>
    [Cached(typeof(OsuGame))]
    public partial class OsuGame : OsuGameBase, IKeyBindingHandler<GlobalAction>, ILocalUserPlayInfo, IPerformFromScreenRunner, IOverlayManager, ILinkHandler
    {
#if DEBUG
        // Different port allows runnning release and debug builds alongside each other.
        public const int IPC_PORT = 44824;
#else
        public const int IPC_PORT = 44823;
#endif

        /// <summary>
        /// The amount of global offset to apply when a left/right anchored overlay is displayed (ie. settings or notifications).
        /// </summary>
        protected const float SIDE_OVERLAY_OFFSET_RATIO = 0.05f;

        public Toolbar Toolbar { get; private set; }

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
        /// Whether the user is currently in an idle state.
        /// </summary>
        public IBindable<bool> IsIdle => idleTracker.IsIdle;

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

        protected FirstRunSetupOverlay FirstRunOverlay { get; private set; }

        private FPSCounter fpsCounter;

        private VolumeOverlay volume;

        private OsuLogo osuLogo;

        private MainMenu menuScreen;

        private VersionManager versionManager;

        [CanBeNull]
        private IntroScreen introScreen;

        private Bindable<string> configRuleset;

        private Bindable<bool> applySafeAreaConsiderations;

        private Bindable<float> uiScale;

        private Bindable<string> configSkin;

        private readonly string[] args;

        private readonly List<OsuFocusedOverlayContainer> focusedOverlays = new List<OsuFocusedOverlayContainer>();
        private readonly List<OverlayContainer> externalOverlays = new List<OverlayContainer>();

        private readonly List<OverlayContainer> visibleBlockingOverlays = new List<OverlayContainer>();

        public OsuGame(string[] args = null)
        {
            this.args = args;

            forwardGeneralLogsToNotifications();
            forwardTabletLogsToNotifications();

            SentryLogger = new SentryLogger(this);
        }

        #region IOverlayManager

        IBindable<OverlayActivation> IOverlayManager.OverlayActivationMode => OverlayActivationMode;

        private void updateBlockingOverlayFade() =>
            ScreenContainer.FadeColour(visibleBlockingOverlays.Any() ? OsuColour.Gray(0.5f) : Color4.White, 500, Easing.OutQuint);

        IDisposable IOverlayManager.RegisterBlockingOverlay(OverlayContainer overlayContainer)
        {
            if (overlayContainer.Parent != null)
                throw new ArgumentException($@"Overlays registered via {nameof(IOverlayManager.RegisterBlockingOverlay)} should not be added to the scene graph.");

            if (externalOverlays.Contains(overlayContainer))
                throw new ArgumentException($@"{overlayContainer} has already been registered via {nameof(IOverlayManager.RegisterBlockingOverlay)} once.");

            externalOverlays.Add(overlayContainer);
            overlayContent.Add(overlayContainer);

            if (overlayContainer is OsuFocusedOverlayContainer focusedOverlayContainer)
                focusedOverlays.Add(focusedOverlayContainer);

            return new InvokeOnDisposal(() => unregisterBlockingOverlay(overlayContainer));
        }

        void IOverlayManager.ShowBlockingOverlay(OverlayContainer overlay)
        {
            if (!visibleBlockingOverlays.Contains(overlay))
                visibleBlockingOverlays.Add(overlay);
            updateBlockingOverlayFade();
        }

        void IOverlayManager.HideBlockingOverlay(OverlayContainer overlay) => Schedule(() =>
        {
            visibleBlockingOverlays.Remove(overlay);
            updateBlockingOverlayFade();
        });

        /// <summary>
        /// Unregisters a blocking <see cref="OverlayContainer"/> that was not created by <see cref="OsuGame"/> itself.
        /// </summary>
        private void unregisterBlockingOverlay(OverlayContainer overlayContainer) => Schedule(() =>
        {
            externalOverlays.Remove(overlayContainer);

            if (overlayContainer is OsuFocusedOverlayContainer focusedOverlayContainer)
                focusedOverlays.Remove(focusedOverlayContainer);

            overlayContainer.Expire();
        });

        #endregion

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

        protected override UserInputManager CreateUserInputManager()
        {
            var userInputManager = base.CreateUserInputManager();
            (userInputManager as OsuUserInputManager)?.LocalUserPlaying.BindTo(LocalUserPlaying);
            return userInputManager;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private readonly List<string> dragDropFiles = new List<string>();
        private ScheduledDelegate dragDropImportSchedule;

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            if (host.Window != null)
            {
                host.Window.DragDrop += path =>
                {
                    // on macOS/iOS, URL associations are handled via SDL_DROPFILE events.
                    if (path.StartsWith(OSU_PROTOCOL, StringComparison.Ordinal))
                    {
                        HandleLink(path);
                        return;
                    }

                    lock (dragDropFiles)
                    {
                        dragDropFiles.Add(path);

                        Logger.Log($@"Adding ""{Path.GetFileName(path)}"" for import");

                        // File drag drop operations can potentially trigger hundreds or thousands of these calls on some platforms.
                        // In order to avoid spawning multiple import tasks for a single drop operation, debounce a touch.
                        dragDropImportSchedule?.Cancel();
                        dragDropImportSchedule = Scheduler.AddDelayed(handlePendingDragDropImports, 100);
                    }
                };
            }
        }

        private void handlePendingDragDropImports()
        {
            lock (dragDropFiles)
            {
                Logger.Log($"Handling batch import of {dragDropFiles.Count} files");

                string[] paths = dragDropFiles.ToArray();
                dragDropFiles.Clear();

                Task.Factory.StartNew(() => Import(paths), TaskCreationOptions.LongRunning);
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SentryLogger.AttachUser(API.LocalUser);

            dependencies.Cache(osuLogo = new OsuLogo { Alpha = 0 });

            // bind config int to database RulesetInfo
            configRuleset = LocalConfig.GetBindable<string>(OsuSetting.Ruleset);
            uiScale = LocalConfig.GetBindable<float>(OsuSetting.UIScale);

            var preferredRuleset = RulesetStore.GetRuleset(configRuleset.Value);

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

            configSkin = LocalConfig.GetBindable<string>(OsuSetting.Skin);

            // Transfer skin from config to realm instance once on startup.
            SkinManager.SetSkinFromConfiguration(configSkin.Value);

            // Transfer any runtime changes back to configuration file.
            SkinManager.CurrentSkinInfo.ValueChanged += skin => configSkin.Value = skin.NewValue.ID.ToString();

            LocalUserPlaying.BindValueChanged(p =>
            {
                BeatmapManager.PauseImports = p.NewValue;
                SkinManager.PauseImports = p.NewValue;
                ScoreManager.PauseImports = p.NewValue;
            }, true);

            IsActive.BindValueChanged(active => updateActiveState(active.NewValue), true);

            Audio.AddAdjustment(AdjustableProperty.Volume, inactiveVolumeFade);

            SelectedMods.BindValueChanged(modsChanged);
            Beatmap.BindValueChanged(beatmapChanged, true);

            applySafeAreaConsiderations = LocalConfig.GetBindable<bool>(OsuSetting.SafeAreaConsiderations);
            applySafeAreaConsiderations.BindValueChanged(apply => SafeAreaContainer.SafeAreaOverrideEdges = apply.NewValue ? SafeAreaOverrideEdges : Edges.All, true);
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
            string argString = link.Argument.ToString() ?? string.Empty;

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
                    if (link.Argument is RomanisableString romanisable)
                        SearchBeatmapSet(romanisable.GetPreferred(Localisation.CurrentParameters.Value.PreferOriginalScript));
                    else
                        SearchBeatmapSet(argString);
                    break;

                case LinkAction.FilterBeatmapSetGenre:
                    FilterBeatmapSetGenre((SearchGenre)link.Argument);
                    break;

                case LinkAction.FilterBeatmapSetLanguage:
                    FilterBeatmapSetLanguage((SearchLanguage)link.Argument);
                    break;

                case LinkAction.OpenEditorTimestamp:
                    HandleTimestamp(argString);
                    break;

                case LinkAction.JoinMultiplayerMatch:
                case LinkAction.Spectate:
                    waitForReady(() => Notifications, _ => Notifications.Post(new SimpleNotification
                    {
                        Text = NotificationsStrings.LinkTypeNotSupported,
                        Icon = FontAwesome.Solid.LifeRing,
                    }));
                    break;

                case LinkAction.External:
                    OpenUrlExternally(argString);
                    break;

                case LinkAction.OpenUserProfile:
                    ShowUser((IUser)link.Argument);
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

            if (!url.CheckIsValidUrl())
            {
                Notifications.Post(new SimpleErrorNotification
                {
                    Text = NotificationsStrings.UnsupportedOrDangerousUrlProtocol(url),
                });

                return;
            }

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

        public void FilterBeatmapSetGenre(SearchGenre genre) => waitForReady(() => beatmapListing, _ => beatmapListing.ShowWithGenreFilter(genre));

        public void FilterBeatmapSetLanguage(SearchLanguage language) => waitForReady(() => beatmapListing, _ => beatmapListing.ShowWithLanguageFilter(language));

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
        /// Seeks to the provided <paramref name="timestamp"/> if the editor is currently open.
        /// Can also select objects as indicated by the <paramref name="timestamp"/> (depends on ruleset implementation).
        /// </summary>
        public void HandleTimestamp(string timestamp)
        {
            if (ScreenStack.CurrentScreen is not Editor editor)
            {
                Schedule(() => Notifications.Post(new SimpleErrorNotification
                {
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                    Text = EditorStrings.MustBeInEditorToHandleLinks
                }));
                return;
            }

            editor.HandleTimestamp(timestamp);
        }

        /// <summary>
        /// Present a skin select immediately.
        /// </summary>
        /// <param name="skin">The skin to select.</param>
        public void PresentSkin(SkinInfo skin)
        {
            var databasedSkin = SkinManager.Query(s => s.ID == skin.ID);

            if (databasedSkin == null)
            {
                Logger.Log("The requested skin could not be loaded.", LoggingTarget.Information);
                return;
            }

            SkinManager.CurrentSkinInfo.Value = databasedSkin;
        }

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
            Logger.Log($"Beginning {nameof(PresentBeatmap)} with beatmap {beatmap}");
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

            if (detachedSet.DeletePending)
            {
                Logger.Log("The requested beatmap has since been deleted.", LoggingTarget.Information);
                return;
            }

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
                    // Don't change the local ruleset if the user is on another ruleset and is showing converted beatmaps at song select.
                    // Eventually we probably want to check whether conversion is actually possible for the current ruleset.
                    bool requiresRulesetSwitch = !selection.Ruleset.Equals(Ruleset.Value)
                                                 && (selection.Ruleset.OnlineID > 0 || !LocalConfig.Get<bool>(OsuSetting.ShowConvertedBeatmaps));

                    if (requiresRulesetSwitch)
                    {
                        Ruleset.Value = selection.Ruleset;
                        Beatmap.Value = BeatmapManager.GetWorkingBeatmap(selection);

                        Logger.Log($"Completing {nameof(PresentBeatmap)} with beatmap {beatmap} ruleset {selection.Ruleset}");
                    }
                    else
                    {
                        Beatmap.Value = BeatmapManager.GetWorkingBeatmap(selection);

                        Logger.Log($"Completing {nameof(PresentBeatmap)} with beatmap {beatmap} (maintaining ruleset)");
                    }
                }
            }, validScreens: new[]
            {
                typeof(SongSelect), typeof(IHandlePresentBeatmap)
            });
        }

        /// <summary>
        /// Join a multiplayer match immediately.
        /// </summary>
        /// <param name="room">The room to join.</param>
        /// <param name="password">The password to join the room, if any is given.</param>
        public void PresentMultiplayerMatch(Room room, string password)
        {
            PerformFromScreen(screen =>
            {
                if (!(screen is Multiplayer multiplayer))
                    screen.Push(multiplayer = new Multiplayer());

                multiplayer.Join(room, password);
            });
            // TODO: We should really be able to use `validScreens: new[] { typeof(Multiplayer) }` here
            // but `PerformFromScreen` doesn't understand nested stacks.
        }

        /// <summary>
        /// Present a score's replay immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        public void PresentScore(IScoreInfo score, ScorePresentType presentType = ScorePresentType.Results)
        {
            Logger.Log($"Beginning {nameof(PresentScore)} with score {score}");

            // The given ScoreInfo may have missing properties if it was retrieved from online data. Re-retrieve it from the database
            // to ensure all the required data for presenting a replay are present.
            ScoreInfo databasedScoreInfo = null;

            if (score.OnlineID > 0)
                databasedScoreInfo = ScoreManager.Query(s => s.OnlineID == score.OnlineID);

            if (score.LegacyOnlineID > 0)
                databasedScoreInfo ??= ScoreManager.Query(s => s.LegacyOnlineID == score.LegacyOnlineID);

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

            // This should be able to be performed from song select, but that is disabled for now
            // due to the weird decoupled ruleset logic (which can cause a crash in certain filter scenarios).
            //
            // As a special case, if the beatmap and ruleset already match, allow immediately displaying the score from song select.
            // This is guaranteed to not crash, and feels better from a user's perspective (ie. if they are clicking a score in the
            // song select leaderboard).
            IEnumerable<Type> validScreens =
                Beatmap.Value.BeatmapInfo.Equals(databasedBeatmap) && Ruleset.Value.Equals(databasedScore.ScoreInfo.Ruleset)
                    ? new[] { typeof(SongSelect) }
                    : Array.Empty<Type>();

            PerformFromScreen(screen =>
            {
                Logger.Log($"{nameof(PresentScore)} updating beatmap ({databasedBeatmap}) and ruleset ({databasedScore.ScoreInfo.Ruleset}) to match score");

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
            }, validScreens: validScreens);
        }

        public override Task Import(ImportTask[] imports, ImportParameters parameters = default)
        {
            // encapsulate task as we don't want to begin the import process until in a ready state.

            // ReSharper disable once AsyncVoidLambda
            // TODO: This is bad because `new Task` doesn't have a Func<Task?> override.
            // Only used for android imports and a bit of a mess. Probably needs rethinking overall.
            var importTask = new Task(async () => await base.Import(imports, parameters).ConfigureAwait(false));

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

        public void PerformFromScreen(Action<IScreen> action, IEnumerable<Type> validScreens = null)
        {
            performFromMainMenuTask?.Cancel();
            Add(performFromMainMenuTask = new PerformFromMenuRunner(action, validScreens, () => ScreenStack.CurrentScreen));
        }

        public override void AttemptExit()
        {
            // The main menu exit implementation gives the user a chance to interrupt the exit process if needed.
            PerformFromScreen(menu => menu.Exit(), new[] { typeof(MainMenu) });
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
        {
            return new Dictionary<FrameworkSetting, object>
            {
                // General expectation that osu! starts in fullscreen by default (also gives the most predictable performance).
                // However, macOS is bound to have issues when using exclusive fullscreen as it takes full control away from OS, therefore borderless is default there.
                { FrameworkSetting.WindowMode, RuntimeInfo.OS == RuntimeInfo.Platform.macOS ? WindowMode.Borderless : WindowMode.Fullscreen }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var languages = Enum.GetValues<Language>();

            var mappings = languages.Select(language =>
            {
#if DEBUG
                if (language == Language.debug)
                    return new LocaleMapping("debug", new DebugLocalisationStore());
#endif

                string cultureCode = language.ToCultureCode();

                try
                {
                    return new LocaleMapping(new ResourceManagerLocalisationStore(cultureCode));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Could not load localisations for language \"{cultureCode}\"");
                    return null;
                }
            }).Where(m => m != null);

            Localisation.AddLocaleMappings(mappings);

            // The next time this is updated is in UpdateAfterChildren, which occurs too late and results
            // in the cursor being shown for a few frames during the intro.
            // This prevents the cursor from showing until we have a screen with CursorVisible = true
            GlobalCursorDisplay.ShowCursor = menuScreen?.CursorVisible ?? false;

            // todo: all archive managers should be able to be looped here.
            SkinManager.PostNotification = n => Notifications.Post(n);
            SkinManager.PresentImport = items => PresentSkin(items.First().Value);

            BeatmapManager.PostNotification = n => Notifications.Post(n);
            BeatmapManager.PresentImport = items => PresentBeatmap(items.First().Value);

            BeatmapDownloader.PostNotification = n => Notifications.Post(n);
            ScoreDownloader.PostNotification = n => Notifications.Post(n);

            ScoreManager.PostNotification = n => Notifications.Post(n);
            ScoreManager.PresentImport = items => PresentScore(items.First().Value);

            MultiplayerClient.PostNotification = n => Notifications.Post(n);
            MultiplayerClient.PresentMatch = PresentMultiplayerMatch;

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
                        leftFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                        rightFloatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                    }
                },
                topMostOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                idleTracker,
                new ConfineMouseTracker()
            });

            ScreenStack.ScreenPushed += screenPushed;
            ScreenStack.ScreenExited += screenExited;

            loadComponentSingleFile(fpsCounter = new FPSCounter
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(5),
            }, topMostOverlayContent.Add);

            if (!IsDeployedBuild)
            {
                dependencies.Cache(versionManager = new VersionManager { Depth = int.MinValue });
                loadComponentSingleFile(versionManager, ScreenContainer.Add);
            }

            loadComponentSingleFile(osuLogo, _ =>
            {
                osuLogo.SetupDefaultContainer(logoContainer);

                // Loader has to be created after the logo has finished loading as Loader performs logo transformations on entering.
                ScreenStack.Push(CreateLoader().With(l => l.RelativeSizeAxes = Axes.Both));
            });

            loadComponentSingleFile(new SoloStatisticsWatcher(), Add, true);
            loadComponentSingleFile(Toolbar = new Toolbar
            {
                OnHome = delegate
                {
                    CloseAllOverlays(false);

                    if (menuScreen?.GetChildScreen() != null)
                        menuScreen.MakeCurrent();
                },
            }, topMostOverlayContent.Add);

            loadComponentSingleFile(volume = new VolumeOverlay(), leftFloatingOverlayContent.Add, true);

            var onScreenDisplay = new OnScreenDisplay();

            onScreenDisplay.BeginTracking(this, frameworkConfig);
            onScreenDisplay.BeginTracking(this, LocalConfig);

            loadComponentSingleFile(onScreenDisplay, Add, true);

            loadComponentSingleFile<INotificationOverlay>(Notifications.With(d =>
            {
                d.Anchor = Anchor.TopRight;
                d.Origin = Anchor.TopRight;
            }), rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(legacyImportManager, Add);

            loadComponentSingleFile(screenshotManager, Add);

            // dependency on notification overlay, dependent by settings overlay
            loadComponentSingleFile(CreateUpdateManager(), Add, true);

            // overlay elements
            loadComponentSingleFile(FirstRunOverlay = new FirstRunSetupOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(new ManageCollectionsDialog(), overlayContent.Add, true);
            loadComponentSingleFile(beatmapListing = new BeatmapListingOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(dashboard = new DashboardOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(news = new NewsOverlay(), overlayContent.Add, true);
            var rankingsOverlay = loadComponentSingleFile(new RankingsOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(channelManager = new ChannelManager(API), Add, true);
            loadComponentSingleFile(chatOverlay = new ChatOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(new MessageNotifier(), Add, true);
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
            loadComponentSingleFile<IDialogOverlay>(new DialogOverlay(), topMostOverlayContent.Add, true);

            loadComponentSingleFile(CreateHighPerformanceSession(), Add);

            loadComponentSingleFile(new BackgroundDataStoreProcessor(), Add);

            Add(difficultyRecommender);
            Add(externalLinkOpener = new ExternalLinkOpener());
            Add(new MusicKeyBindingHandler());
            Add(new OnlineStatusNotifier(() => ScreenStack.CurrentScreen));

            // side overlays which cancel each other.
            var singleDisplaySideOverlays = new OverlayContainer[] { Settings, Notifications, FirstRunOverlay };

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
            var singleDisplayOverlays = new OverlayContainer[] { FirstRunOverlay, chatOverlay, news, dashboard, beatmapListing, changelogOverlay, rankingsOverlay, wikiOverlay };

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
                {
                    string firstPath = paths.First();

                    if (firstPath.StartsWith(OSU_PROTOCOL, StringComparison.Ordinal))
                    {
                        HandleLink(firstPath);
                    }
                    else
                    {
                        Task.Run(() => Import(paths));
                    }
                }
            }
        }

        private void showOverlayAboveOthers(OverlayContainer overlay, OverlayContainer[] otherOverlays)
        {
            otherOverlays.Where(o => o != overlay).ForEach(o => o.Hide());

            Settings.Hide();
            Notifications.Hide();

            // Partially visible so leave it at the current depth.
            if (overlay.IsPresent)
                return;

            // Show above all other overlays.
            if (overlay.IsLoaded)
                overlayContent.ChangeChildDepth(overlay, (float)-Clock.CurrentTime);
            else
                overlay.Depth = (float)-Clock.CurrentTime;
        }

        private void forwardGeneralLogsToNotifications()
        {
            int recentLogCount = 0;

            const double debounce = 60000;

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important || entry.Target > LoggingTarget.Database || entry.Target == null) return;

                if (entry.Exception is SentryOnlyDiagnosticsException)
                    return;

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
                    string logFile = Logger.GetLogger(entry.Target.Value).Filename;

                    Schedule(() => Notifications.Post(new SimpleNotification
                    {
                        Icon = FontAwesome.Solid.EllipsisH,
                        Text = NotificationsStrings.SubsequentMessagesLogged,
                        Activated = () =>
                        {
                            Logger.Storage.PresentFileExternally(logFile);
                            return true;
                        }
                    }));
                }

                Interlocked.Increment(ref recentLogCount);
                Scheduler.AddDelayed(() => Interlocked.Decrement(ref recentLogCount), debounce);
            };
        }

        private void forwardTabletLogsToNotifications()
        {
            const string tablet_prefix = @"[Tablet] ";

            bool notifyOnWarning = true;
            bool notifyOnError = true;

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important || entry.Target != LoggingTarget.Input || !entry.Message.StartsWith(tablet_prefix, StringComparison.OrdinalIgnoreCase))
                    return;

                string message = entry.Message.Replace(tablet_prefix, string.Empty);

                if (entry.Level == LogLevel.Error)
                {
                    if (!notifyOnError)
                        return;

                    notifyOnError = false;

                    Schedule(() =>
                    {
                        Notifications.Post(new SimpleNotification
                        {
                            Text = NotificationsStrings.TabletSupportDisabledDueToError(message),
                            Icon = FontAwesome.Solid.PenSquare,
                            IconColour = Colours.RedDark,
                        });

                        // We only have one tablet handler currently.
                        // The loop here is weakly guarding against a future where more than one is added.
                        // If this is ever the case, this logic needs adjustment as it should probably only
                        // disable the relevant tablet handler rather than all.
                        foreach (var tabletHandler in Host.AvailableInputHandlers.OfType<ITabletHandler>())
                            tabletHandler.Enabled.Value = false;
                    });
                }
                else if (notifyOnWarning)
                {
                    Schedule(() => Notifications.Post(new SimpleNotification
                    {
                        Text = NotificationsStrings.EncounteredTabletWarning,
                        Icon = FontAwesome.Solid.PenSquare,
                        IconColour = Colours.YellowDark,
                        Activated = () =>
                        {
                            OpenUrlExternally("https://opentabletdriver.net/Tablets", true);
                            return true;
                        }
                    }));

                    notifyOnWarning = false;
                }
            };

            Schedule(() =>
            {
                ITabletHandler tablet = Host.AvailableInputHandlers.OfType<ITabletHandler>().SingleOrDefault();
                tablet?.Tablet.BindValueChanged(_ =>
                {
                    notifyOnWarning = true;
                    notifyOnError = true;
                }, true);
            });
        }

        private Task asyncLoadStream;

        /// <summary>
        /// Queues loading the provided component in sequential fashion.
        /// This operation is limited to a single thread to avoid saturating all cores.
        /// </summary>
        /// <param name="component">The component to load.</param>
        /// <param name="loadCompleteAction">An action to invoke on load completion (generally to add the component to the hierarchy).</param>
        /// <param name="cache">Whether to cache the component as type <typeparamref name="T"/> into the game dependencies before any scheduling.</param>
        private T loadComponentSingleFile<T>(T component, Action<Drawable> loadCompleteAction, bool cache = false)
            where T : class
        {
            if (cache)
                dependencies.CacheAs(component);

            var drawableComponent = component as Drawable ?? throw new ArgumentException($"Component must be a {nameof(Drawable)}", nameof(component));

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
                        var del = new ScheduledDelegate(() => task = LoadComponentAsync(drawableComponent, loadCompleteAction));
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
                case GlobalAction.ToggleFPSDisplay:
                    fpsCounter.ToggleVisibility();
                    return true;

                case GlobalAction.ToggleSkinEditor:
                    skinEditor.ToggleVisibility();
                    return true;

                case GlobalAction.ResetInputSettings:
                    Host.ResetInputHandlers();
                    frameworkConfig.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode).SetDefault();
                    return true;

                case GlobalAction.ToggleGameplayMouseButtons:
                    var mouseDisableButtons = LocalConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons);
                    mouseDisableButtons.Value = !mouseDisableButtons.Value;
                    return true;

                case GlobalAction.ToggleProfile:
                    if (userProfile.State.Value == Visibility.Visible)
                        userProfile.Hide();
                    else
                        ShowUser(API.LocalUser.Value);
                    return true;

                case GlobalAction.RandomSkin:
                    // Don't allow random skin selection while in the skin editor.
                    // This is mainly to stop many "osu! default (modified)" skins being created via the SkinManager.EnsureMutableSkin() path.
                    // If people want this to work we can potentially avoid selecting default skins when the editor is open, or allow a maximum of one mutable skin somehow.
                    if (skinEditor.State.Value == Visibility.Visible)
                        return false;

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
            overlayContent.X = horizontalOffset * 1.2f;

            GlobalCursorDisplay.ShowCursor = (ScreenStack.CurrentScreen as IOsuScreen)?.CursorVisible ?? false;
        }

        private void screenChanged(IScreen current, IScreen newScreen)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts[@"screen stack"] = new
                {
                    Current = newScreen?.GetType().ReadableName(),
                    Previous = current?.GetType().ReadableName(),
                };

                scope.SetTag(@"screen", newScreen?.GetType().ReadableName() ?? @"none");
            });

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

                GlobalCursorDisplay.MenuCursor.HideCursorOnNonMouseInput = newOsuScreen.HideMenuCursorOnNonMouseInput;

                if (newOsuScreen.HideOverlaysOnEnter)
                    CloseAllOverlays();
                else
                    Toolbar.Show();

                if (newOsuScreen.AllowBackButton)
                    BackButton.Show();
                else
                    BackButton.Hide();
            }

            skinEditor.SetTarget((OsuScreen)newScreen);
        }

        private void screenPushed(IScreen lastScreen, IScreen newScreen) => screenChanged(lastScreen, newScreen);

        private void screenExited(IScreen lastScreen, IScreen newScreen)
        {
            screenChanged(lastScreen, newScreen);

            if (newScreen == null)
                Exit();
        }

        IBindable<bool> ILocalUserPlayInfo.IsPlaying => LocalUserPlaying;
    }
}
