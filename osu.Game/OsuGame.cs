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
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Input.Bindings;
using osu.Game.Online.Chat;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
using osuTK.Graphics;
using osu.Game.Overlays.Volume;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Utils;
using LogLevel = osu.Framework.Logging.LogLevel;

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

        private MusicController musicController;

        private NotificationOverlay notifications;

        private LoginOverlay loginOverlay;

        private DialogOverlay dialogOverlay;

        private AccountCreationOverlay accountCreation;

        private DirectOverlay direct;

        private SocialOverlay social;

        private UserProfileOverlay userProfile;

        private BeatmapSetOverlay beatmapSetOverlay;

        [Cached]
        private readonly ScreenshotManager screenshotManager = new ScreenshotManager();

        protected RavenLogger RavenLogger;

        public virtual Storage GetStorageForStableInstall() => null;

        public float ToolbarOffset => Toolbar.Position.Y + Toolbar.DrawHeight;

        private IdleTracker idleTracker;

        public readonly Bindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>();

        private OsuScreenStack screenStack;
        private VolumeOverlay volume;
        private OnScreenDisplay onscreenDisplay;
        private OsuLogo osuLogo;

        private MainMenu menuScreen;
        private Intro introScreen;

        private Bindable<int> configRuleset;
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private Bindable<int> configSkin;

        private readonly string[] args;

        private SettingsOverlay settings;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        private readonly List<OverlayContainer> visibleBlockingOverlays = new List<OverlayContainer>();

        // todo: move this to SongSelect once Screen has the ability to unsuspend.
        [Cached]
        [Cached(Type = typeof(IBindable<IEnumerable<Mod>>))]
        private readonly Bindable<IEnumerable<Mod>> selectedMods = new Bindable<IEnumerable<Mod>>(new Mod[] { });

        public OsuGame(string[] args = null)
        {
            this.args = args;

            forwardLoggedErrorsToNotifications();

            RavenLogger = new RavenLogger(this);
        }

        public void ToggleSettings() => settings.ToggleVisibility();

        public void ToggleDirect() => direct.ToggleVisibility();

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
        /// <param name="toolbar">Whether the toolbar should also be hidden.</param>
        public void CloseAllOverlays(bool toolbar = true)
        {
            foreach (var overlay in overlays)
                overlay.State = Visibility.Hidden;
            if (toolbar) Toolbar.State = Visibility.Hidden;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            this.frameworkConfig = frameworkConfig;

            if (!Host.IsPrimaryInstance)
            {
                Logger.Log(@"osu! does not support multiple running instances.", LoggingTarget.Runtime, LogLevel.Error);
                Environment.Exit(0);
            }

            if (args?.Length > 0)
            {
                var paths = args.Where(a => !a.StartsWith(@"-")).ToArray();
                if (paths.Length > 0)
                    Task.Run(() => Import(paths));
            }

            dependencies.CacheAs(this);

            dependencies.Cache(RavenLogger);

            dependencies.CacheAs(ruleset);
            dependencies.CacheAs<IBindable<RulesetInfo>>(ruleset);

            dependencies.Cache(osuLogo = new OsuLogo { Alpha = 0 });

            // bind config int to database RulesetInfo
            configRuleset = LocalConfig.GetBindable<int>(OsuSetting.Ruleset);
            ruleset.Value = RulesetStore.GetRuleset(configRuleset.Value) ?? RulesetStore.AvailableRulesets.First();
            ruleset.ValueChanged += r => configRuleset.Value = r.NewValue.ID ?? 0;

            // bind config int to database SkinInfo
            configSkin = LocalConfig.GetBindable<int>(OsuSetting.Skin);
            SkinManager.CurrentSkinInfo.ValueChanged += skin => configSkin.Value = skin.NewValue.ID;
            configSkin.ValueChanged += skinId => SkinManager.CurrentSkinInfo.Value = SkinManager.Query(s => s.ID == skinId.NewValue) ?? SkinInfo.Default;
            configSkin.TriggerChange();

            LocalConfig.BindWith(OsuSetting.VolumeInactive, inactiveVolumeAdjust);

            IsActive.BindValueChanged(active => updateActiveState(active.NewValue), true);
        }

        private ExternalLinkOpener externalLinkOpener;

        public void OpenUrlExternally(string url)
        {
            if (url.StartsWith("/"))
                url = $"{API.Endpoint}{url}";

            externalLinkOpener.OpenUrlExternally(url);
        }

        /// <summary>
        /// Show a beatmap set as an overlay.
        /// </summary>
        /// <param name="setId">The set to display.</param>
        public void ShowBeatmapSet(int setId) => beatmapSetOverlay.FetchAndShowBeatmapSet(setId);

        /// <summary>
        /// Show a user's profile as an overlay.
        /// </summary>
        /// <param name="userId">The user to display.</param>
        public void ShowUser(long userId) => userProfile.ShowUser(userId);

        /// <summary>
        /// Show a beatmap's set as an overlay, displaying the given beatmap.
        /// </summary>
        /// <param name="beatmapId">The beatmap to show.</param>
        public void ShowBeatmap(int beatmapId) => beatmapSetOverlay.FetchAndShowBeatmap(beatmapId);

        /// <summary>
        /// Present a beatmap at song select immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        /// <param name="beatmap">The beatmap to select.</param>
        public void PresentBeatmap(BeatmapSetInfo beatmap)
        {
            var databasedSet = beatmap.OnlineBeatmapSetID != null
                ? BeatmapManager.QueryBeatmapSet(s => s.OnlineBeatmapSetID == beatmap.OnlineBeatmapSetID)
                : BeatmapManager.QueryBeatmapSet(s => s.Hash == beatmap.Hash);

            if (databasedSet == null)
            {
                Logger.Log("The requested beatmap could not be loaded.", LoggingTarget.Information);
                return;
            }

            performFromMainMenu(() =>
            {
                // we might already be at song select, so a check is required before performing the load to solo.
                if (menuScreen.IsCurrentScreen())
                    menuScreen.LoadToSolo();

                // Use first beatmap available for current ruleset, else switch ruleset.
                var first = databasedSet.Beatmaps.Find(b => b.Ruleset == ruleset.Value) ?? databasedSet.Beatmaps.First();

                ruleset.Value = first.Ruleset;
                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(first);
            }, $"load {beatmap}", bypassScreenAllowChecks: true, targetScreen: typeof(PlaySongSelect));
        }

        /// <summary>
        /// Present a score's replay immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        /// <param name="beatmap">The beatmap to select.</param>
        public void PresentScore(ScoreInfo score)
        {
            var databasedScore = ScoreManager.GetScore(score);
            var databasedScoreInfo = databasedScore.ScoreInfo;
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

            performFromMainMenu(() =>
            {
                ruleset.Value = databasedScoreInfo.Ruleset;

                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(databasedBeatmap);
                Beatmap.Value.Mods.Value = databasedScoreInfo.Mods;

                menuScreen.Push(new PlayerLoader(() => new ReplayPlayer(databasedScore)));
            }, $"watch {databasedScoreInfo}", bypassScreenAllowChecks: true);
        }

        private ScheduledDelegate performFromMainMenuTask;

        /// <summary>
        /// Perform an action only after returning to the main menu.
        /// Eagerly tries to exit the current screen until it succeeds.
        /// </summary>
        /// <param name="action">The action to perform once we are in the correct state.</param>
        /// <param name="taskName">The task name to display in a notification (if we can't immediately reach the main menu state).</param>
        /// <param name="targetScreen">An optional target screen type. If this screen is already current we can immediately perform the action without returning to the menu.</param>
        /// <param name="bypassScreenAllowChecks">Whether checking <see cref="IOsuScreen.AllowExternalScreenChange"/> should be bypassed.</param>
        private void performFromMainMenu(Action action, string taskName, Type targetScreen = null, bool bypassScreenAllowChecks = false)
        {
            performFromMainMenuTask?.Cancel();

            // if the current screen does not allow screen changing, give the user an option to try again later.
            if (!bypassScreenAllowChecks && (screenStack.CurrentScreen as IOsuScreen)?.AllowExternalScreenChange == false)
            {
                notifications.Post(new SimpleNotification
                {
                    Text = $"Click here to {taskName}",
                    Activated = () =>
                    {
                        performFromMainMenu(action, taskName, targetScreen, true);
                        return true;
                    }
                });

                return;
            }

            CloseAllOverlays(false);

            // we may already be at the target screen type.
            if (targetScreen != null && screenStack.CurrentScreen?.GetType() == targetScreen)
            {
                action();
                return;
            }

            // all conditions have been met to continue with the action.
            if (menuScreen?.IsCurrentScreen() == true && !Beatmap.Disabled)
            {
                action();
                return;
            }

            // menuScreen may not be initialised yet (null check required).
            menuScreen?.MakeCurrent();

            performFromMainMenuTask = Schedule(() => performFromMainMenu(action, taskName));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            RavenLogger.Dispose();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // The next time this is updated is in UpdateAfterChildren, which occurs too late and results
            // in the cursor being shown for a few frames during the intro.
            // This prevents the cursor from showing until we have a screen with CursorVisible = true
            MenuCursorContainer.CanShowCursor = menuScreen?.CursorVisible ?? false;

            // todo: all archive managers should be able to be looped here.
            SkinManager.PostNotification = n => notifications?.Post(n);
            SkinManager.GetStableStorage = GetStorageForStableInstall;

            BeatmapManager.PostNotification = n => notifications?.Post(n);
            BeatmapManager.GetStableStorage = GetStorageForStableInstall;
            BeatmapManager.PresentImport = items => PresentBeatmap(items.First());

            ScoreManager.PostNotification = n => notifications?.Post(n);
            ScoreManager.PresentImport = items => PresentScore(items.First());

            Container logoContainer;

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
                        screenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both },
                        logoContainer = new Container { RelativeSizeAxes = Axes.Both },
                    }
                },
                overlayContent = new Container { RelativeSizeAxes = Axes.Both },
                floatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                topMostOverlayContent = new Container { RelativeSizeAxes = Axes.Both },
                idleTracker = new GameIdleTracker(6000)
            });

            screenStack.ScreenPushed += screenPushed;
            screenStack.ScreenExited += screenExited;

            loadComponentSingleFile(osuLogo, logo =>
            {
                logoContainer.Add(logo);

                // Loader has to be created after the logo has finished loading as Loader performs logo transformations on entering.
                screenStack.Push(new Loader
                {
                    RelativeSizeAxes = Axes.Both
                });
            });

            loadComponentSingleFile(Toolbar = new Toolbar
            {
                OnHome = delegate
                {
                    CloseAllOverlays(false);
                    menuScreen?.MakeCurrent();
                },
            }, topMostOverlayContent.Add);

            loadComponentSingleFile(volume = new VolumeOverlay(), floatingOverlayContent.Add);
            loadComponentSingleFile(onscreenDisplay = new OnScreenDisplay(), Add);

            loadComponentSingleFile(loginOverlay = new LoginOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(screenshotManager, Add);

            //overlay elements
            loadComponentSingleFile(direct = new DirectOverlay(), overlayContent.Add);
            loadComponentSingleFile(social = new SocialOverlay(), overlayContent.Add);
            loadComponentSingleFile(channelManager = new ChannelManager(), AddInternal);
            loadComponentSingleFile(chatOverlay = new ChatOverlay(), overlayContent.Add);
            loadComponentSingleFile(settings = new MainSettings { GetToolbarHeight = () => ToolbarOffset }, floatingOverlayContent.Add);
            loadComponentSingleFile(userProfile = new UserProfileOverlay(), overlayContent.Add);
            loadComponentSingleFile(beatmapSetOverlay = new BeatmapSetOverlay(), overlayContent.Add);

            loadComponentSingleFile(notifications = new NotificationOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(musicController = new MusicController
            {
                GetToolbarHeight = () => ToolbarOffset,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(accountCreation = new AccountCreationOverlay(), topMostOverlayContent.Add);

            loadComponentSingleFile(dialogOverlay = new DialogOverlay(), topMostOverlayContent.Add);

            loadComponentSingleFile(externalLinkOpener = new ExternalLinkOpener(), topMostOverlayContent.Add);

            dependencies.CacheAs(idleTracker);
            dependencies.Cache(settings);
            dependencies.Cache(onscreenDisplay);
            dependencies.Cache(social);
            dependencies.Cache(direct);
            dependencies.Cache(chatOverlay);
            dependencies.Cache(channelManager);
            dependencies.Cache(userProfile);
            dependencies.Cache(musicController);
            dependencies.Cache(beatmapSetOverlay);
            dependencies.Cache(notifications);
            dependencies.Cache(loginOverlay);
            dependencies.Cache(dialogOverlay);
            dependencies.Cache(accountCreation);

            chatOverlay.StateChanged += state => channelManager.HighPollRate.Value = state == Visibility.Visible;

            Add(externalLinkOpener = new ExternalLinkOpener());

            var singleDisplaySideOverlays = new OverlayContainer[] { settings, notifications };
            overlays.AddRange(singleDisplaySideOverlays);

            foreach (var overlay in singleDisplaySideOverlays)
            {
                overlay.StateChanged += state =>
                {
                    if (state == Visibility.Hidden) return;

                    singleDisplaySideOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            // eventually informational overlays should be displayed in a stack, but for now let's only allow one to stay open at a time.
            var informationalOverlays = new OverlayContainer[] { beatmapSetOverlay, userProfile };
            overlays.AddRange(informationalOverlays);

            foreach (var overlay in informationalOverlays)
            {
                overlay.StateChanged += state =>
                {
                    if (state == Visibility.Hidden) return;

                    informationalOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            // ensure only one of these overlays are open at once.
            var singleDisplayOverlays = new OverlayContainer[] { chatOverlay, social, direct };
            overlays.AddRange(singleDisplayOverlays);

            foreach (var overlay in singleDisplayOverlays)
            {
                overlay.StateChanged += state =>
                {
                    // informational overlays should be dismissed on a show or hide of a full overlay.
                    informationalOverlays.ForEach(o => o.Hide());

                    if (state == Visibility.Hidden) return;

                    singleDisplayOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            OverlayActivationMode.ValueChanged += mode =>
            {
                if (mode.NewValue != OverlayActivation.All) CloseAllOverlays();
            };

            void updateScreenOffset()
            {
                float offset = 0;

                if (settings.State == Visibility.Visible)
                    offset += ToolbarButton.WIDTH / 2;
                if (notifications.State == Visibility.Visible)
                    offset -= ToolbarButton.WIDTH / 2;

                screenContainer.MoveToX(offset, SettingsOverlay.TRANSITION_LENGTH, Easing.OutQuint);
            }

            settings.StateChanged += _ => updateScreenOffset();
            notifications.StateChanged += _ => updateScreenOffset();
        }

        public class GameIdleTracker : IdleTracker
        {
            private InputManager inputManager;

            public GameIdleTracker(int time)
                : base(time)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                inputManager = GetContainingInputManager();
            }

            protected override bool AllowIdle => inputManager.FocusedDrawable == null;
        }

        private void forwardLoggedErrorsToNotifications()
        {
            int recentLogCount = 0;

            const double debounce = 5000;

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Important || entry.Target == null) return;

                const int short_term_display_limit = 3;

                if (recentLogCount < short_term_display_limit)
                {
                    Schedule(() => notifications.Post(new SimpleNotification
                    {
                        Icon = entry.Level == LogLevel.Important ? FontAwesome.Solid.ExclamationCircle : FontAwesome.Solid.Bomb,
                        Text = entry.Message + (entry.Exception != null && IsDeployedBuild ? "\n\nThis error has been automatically reported to the devs." : string.Empty),
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
                            Host.Storage.GetStorageForDirectory("logs").OpenInNativeExplorer();
                            return true;
                        }
                    }));
                }

                Interlocked.Increment(ref recentLogCount);
                Scheduler.AddDelayed(() => Interlocked.Decrement(ref recentLogCount), debounce);
            };
        }

        private Task asyncLoadStream;

        private void loadComponentSingleFile<T>(T d, Action<T> add)
            where T : Drawable
        {
            // schedule is here to ensure that all component loads are done after LoadComplete is run (and thus all dependencies are cached).
            // with some better organisation of LoadComplete to do construction and dependency caching in one step, followed by calls to loadComponentSingleFile,
            // we could avoid the need for scheduling altogether.
            Schedule(() =>
            {
                var previousLoadStream = asyncLoadStream;

                //chain with existing load stream
                asyncLoadStream = Task.Run(async () =>
                {
                    if (previousLoadStream != null)
                        await previousLoadStream;

                    try
                    {
                        Logger.Log($"Loading {d}...", level: LogLevel.Debug);

                        // Since this is running in a separate thread, it is possible for OsuGame to be disposed after LoadComponentAsync has been called
                        // throwing an exception. To avoid this, the call is scheduled on the update thread, which does not run if IsDisposed = true
                        Task task = null;
                        var del = new ScheduledDelegate(() => task = LoadComponentAsync(d, add));
                        Scheduler.Add(del);

                        // The delegate won't complete if OsuGame has been disposed in the meantime
                        while (!IsDisposed && !del.Completed)
                            await Task.Delay(10);

                        // Either we're disposed or the load process has started successfully
                        if (IsDisposed)
                            return;

                        Debug.Assert(task != null);

                        await task;

                        Logger.Log($"Loaded {d}!", level: LogLevel.Debug);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });
            });
        }

        public bool OnPressed(GlobalAction action)
        {
            if (introScreen == null) return false;

            switch (action)
            {
                case GlobalAction.ToggleChat:
                    chatOverlay.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleSocial:
                    social.ToggleVisibility();
                    return true;
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
                case GlobalAction.ToggleSettings:
                    settings.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleDirect:
                    direct.ToggleVisibility();
                    return true;
                case GlobalAction.ToggleGameplayMouseButtons:
                    LocalConfig.Set(OsuSetting.MouseDisableButtons, !LocalConfig.Get<bool>(OsuSetting.MouseDisableButtons));
                    return true;
            }

            return false;
        }

        private readonly BindableDouble inactiveVolumeAdjust = new BindableDouble();

        private void updateActiveState(bool isActive)
        {
            if (isActive)
                Audio.RemoveAdjustment(AdjustableProperty.Volume, inactiveVolumeAdjust);
            else
                Audio.AddAdjustment(AdjustableProperty.Volume, inactiveVolumeAdjust);
        }

        public bool OnReleased(GlobalAction action) => false;

        private Container overlayContent;

        private Container floatingOverlayContent;

        private Container topMostOverlayContent;

        private FrameworkConfigManager frameworkConfig;
        private ScalingContainer screenContainer;

        protected override bool OnExiting()
        {
            if (screenStack.CurrentScreen is Loader)
                return false;

            if (introScreen == null)
                return true;

            if (!introScreen.DidLoadMenu || !(screenStack.CurrentScreen is Intro))
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

            MenuCursorContainer.CanShowCursor = (screenStack.CurrentScreen as IOsuScreen)?.CursorVisible ?? false;
        }

        protected virtual void ScreenChanged(IScreen current, IScreen newScreen)
        {
            switch (newScreen)
            {
                case Intro intro:
                    introScreen = intro;
                    break;
                case MainMenu menu:
                    menuScreen = menu;
                    break;
            }

            if (newScreen is IOsuScreen newOsuScreen)
            {
                OverlayActivationMode.Value = newOsuScreen.InitialOverlayActivationMode;

                if (newOsuScreen.HideOverlaysOnEnter)
                    CloseAllOverlays();
                else
                    Toolbar.State = Visibility.Visible;
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
