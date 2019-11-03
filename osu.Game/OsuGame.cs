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
using osu.Framework.Development;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Play;
using osu.Game.Input.Bindings;
using osu.Game.Online.Chat;
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

        private NotificationOverlay notifications;

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

        protected OsuScreenStack ScreenStack;

        protected BackButton BackButton;

        protected SettingsPanel Settings;

        private VolumeOverlay volume;
        private OsuLogo osuLogo;

        private MainMenu menuScreen;

        private IntroScreen introScreen;

        private Bindable<int> configRuleset;

        private Bindable<int> configSkin;

        private readonly string[] args;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        private readonly List<VisibilityContainer> toolbarElements = new List<VisibilityContainer>();

        private readonly List<OverlayContainer> visibleBlockingOverlays = new List<OverlayContainer>();

        public OsuGame(string[] args = null)
        {
            this.args = args;

            forwardLoggedErrorsToNotifications();

            RavenLogger = new RavenLogger(this);
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
        /// <param name="hideToolbarElements">Whether the toolbar (and accompanying controls) should also be hidden.</param>
        public void CloseAllOverlays(bool hideToolbarElements = true)
        {
            foreach (var overlay in overlays)
                overlay.Hide();

            if (hideToolbarElements)
            {
                foreach (var overlay in toolbarElements)
                    overlay.Hide();
            }
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            this.frameworkConfig = frameworkConfig;

            if (!Host.IsPrimaryInstance && !DebugUtils.IsDebugBuild)
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

            Beatmap.BindValueChanged(beatmapChanged, true);
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

                // we might even already be at the song
                if (Beatmap.Value.BeatmapSetInfo.Hash == databasedSet.Hash)
                {
                    return;
                }

                // Use first beatmap available for current ruleset, else switch ruleset.
                var first = databasedSet.Beatmaps.Find(b => b.Ruleset.Equals(Ruleset.Value)) ?? databasedSet.Beatmaps.First();

                Ruleset.Value = first.Ruleset;
                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(first);
            }, $"load {beatmap}", bypassScreenAllowChecks: true, targetScreen: typeof(PlaySongSelect));
        }

        /// <summary>
        /// Present a score's replay immediately.
        /// The user should have already requested this interactively.
        /// </summary>
        public void PresentScore(ScoreInfo score)
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

            performFromMainMenu(() =>
            {
                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(databasedBeatmap);

                menuScreen.Push(new ReplayPlayerLoader(databasedScore));
            }, $"watch {databasedScoreInfo}", bypassScreenAllowChecks: true);
        }

        protected virtual Loader CreateLoader() => new Loader();

        #region Beatmap progression

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            var nextBeatmap = beatmap.NewValue;
            if (nextBeatmap?.Track != null)
                nextBeatmap.Track.Completed += currentTrackCompleted;

            using (var oldBeatmap = beatmap.OldValue)
                if (oldBeatmap?.Track != null)
                    oldBeatmap.Track.Completed -= currentTrackCompleted;

            nextBeatmap?.LoadBeatmapAsync();
        }

        private void currentTrackCompleted()
        {
            if (!Beatmap.Value.Track.Looping && !Beatmap.Disabled)
                musicController.NextTrack();
        }

        #endregion

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
            if (!bypassScreenAllowChecks && (ScreenStack.CurrentScreen as IOsuScreen)?.AllowExternalScreenChange == false)
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
            if (targetScreen != null && ScreenStack.CurrentScreen?.GetType() == targetScreen)
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
            ScoreManager.GetStableStorage = GetStorageForStableInstall;
            ScoreManager.PresentImport = items => PresentScore(items.First());

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
                                if ((ScreenStack.CurrentScreen as IOsuScreen)?.AllowBackButton == true)
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
                idleTracker
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
            }, d =>
            {
                topMostOverlayContent.Add(d);
                toolbarElements.Add(d);
            });

            loadComponentSingleFile(volume = new VolumeOverlay(), leftFloatingOverlayContent.Add, true);

            loadComponentSingleFile(new OnScreenDisplay(), Add, true);

            loadComponentSingleFile(musicController = new MusicController(), Add, true);

            loadComponentSingleFile(notifications = new NotificationOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, rightFloatingOverlayContent.Add, true);

            loadComponentSingleFile(screenshotManager, Add);

            //overlay elements
            loadComponentSingleFile(direct = new DirectOverlay(), overlayContent.Add, true);
            loadComponentSingleFile(social = new SocialOverlay(), overlayContent.Add, true);
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
            }, d =>
            {
                rightFloatingOverlayContent.Add(d);
                toolbarElements.Add(d);
            }, true);

            loadComponentSingleFile(new AccountCreationOverlay(), topMostOverlayContent.Add, true);
            loadComponentSingleFile(new DialogOverlay(), topMostOverlayContent.Add, true);
            loadComponentSingleFile(externalLinkOpener = new ExternalLinkOpener(), topMostOverlayContent.Add);

            chatOverlay.State.ValueChanged += state => channelManager.HighPollRate.Value = state.NewValue == Visibility.Visible;

            Add(externalLinkOpener = new ExternalLinkOpener());

            var singleDisplaySideOverlays = new OverlayContainer[] { Settings, notifications };
            overlays.AddRange(singleDisplaySideOverlays);

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
            overlays.AddRange(informationalOverlays);

            foreach (var overlay in informationalOverlays)
            {
                overlay.State.ValueChanged += state =>
                {
                    if (state.NewValue == Visibility.Hidden) return;

                    informationalOverlays.Where(o => o != overlay).ForEach(o => o.Hide());
                };
            }

            // ensure only one of these overlays are open at once.
            var singleDisplayOverlays = new OverlayContainer[] { chatOverlay, social, direct, changelogOverlay };
            overlays.AddRange(singleDisplayOverlays);

            foreach (var overlay in singleDisplayOverlays)
            {
                overlay.State.ValueChanged += state =>
                {
                    // informational overlays should be dismissed on a show or hide of a full overlay.
                    informationalOverlays.ForEach(o => o.Hide());

                    if (state.NewValue == Visibility.Hidden) return;

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

                if (Settings.State.Value == Visibility.Visible)
                    offset += ToolbarButton.WIDTH / 2;
                if (notifications.State.Value == Visibility.Visible)
                    offset -= ToolbarButton.WIDTH / 2;

                screenContainer.MoveToX(offset, SettingsPanel.TRANSITION_LENGTH, Easing.OutQuint);
            }

            Settings.State.ValueChanged += _ => updateScreenOffset();
            notifications.State.ValueChanged += _ => updateScreenOffset();
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

        private T loadComponentSingleFile<T>(T d, Action<T> add, bool cache = false)
            where T : Drawable
        {
            if (cache)
                dependencies.Cache(d);

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

            return d;
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
                    Settings.ToggleVisibility();
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

        public bool OnReleased(GlobalAction action) => false;

        private Container overlayContent;

        private Container rightFloatingOverlayContent;

        private Container leftFloatingOverlayContent;

        private Container topMostOverlayContent;

        private FrameworkConfigManager frameworkConfig;

        private ScalingContainer screenContainer;

        private MusicController musicController;

        protected override bool OnExiting()
        {
            if (ScreenStack.CurrentScreen is Loader)
                return false;

            if (introScreen == null)
                return true;

            if (!introScreen.DidLoadMenu || !(ScreenStack.CurrentScreen is IntroScreen))
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

            if (newScreen is IOsuScreen newOsuScreen)
            {
                OverlayActivationMode.Value = newOsuScreen.InitialOverlayActivationMode;

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
