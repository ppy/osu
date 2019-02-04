// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
using osuTK;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Audio;
using osu.Framework.Extensions.IEnumerableExtensions;
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

        private BackgroundScreenStack backgroundStack;

        private ParallaxContainer backgroundParallax;

        private ScreenStack screenStack;
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

            ScoreManager.ItemAdded += (score, _, silent) => Schedule(() => LoadScore(score, silent));

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
            ruleset.ValueChanged += r => configRuleset.Value = r.ID ?? 0;

            // bind config int to database SkinInfo
            configSkin = LocalConfig.GetBindable<int>(OsuSetting.Skin);
            SkinManager.CurrentSkinInfo.ValueChanged += s => configSkin.Value = s.ID;
            configSkin.ValueChanged += id => SkinManager.CurrentSkinInfo.Value = SkinManager.Query(s => s.ID == id) ?? SkinInfo.Default;
            configSkin.TriggerChange();

            LocalConfig.BindWith(OsuSetting.VolumeInactive, inactiveVolumeAdjust);
        }

        private ExternalLinkOpener externalLinkOpener;

        public void OpenUrlExternally(string url)
        {
            if (url.StartsWith("/"))
                url = $"{API.Endpoint}{url}";

            externalLinkOpener.OpenUrlExternally(url);
        }

        private ScheduledDelegate scoreLoad;

        /// <summary>
        /// Show a beatmap set as an overlay.
        /// </summary>
        /// <param name="setId">The set to display.</param>
        public void ShowBeatmapSet(int setId) => beatmapSetOverlay.FetchAndShowBeatmapSet(setId);

        /// <summary>
        /// Present a beatmap at song select.
        /// </summary>
        /// <param name="beatmap">The beatmap to select.</param>
        public void PresentBeatmap(BeatmapSetInfo beatmap)
        {
            if (menuScreen == null)
            {
                Schedule(() => PresentBeatmap(beatmap));
                return;
            }

            CloseAllOverlays(false);

            void setBeatmap()
            {
                if (Beatmap.Disabled)
                {
                    Schedule(setBeatmap);
                    return;
                }

                var databasedSet = beatmap.OnlineBeatmapSetID != null ? BeatmapManager.QueryBeatmapSet(s => s.OnlineBeatmapSetID == beatmap.OnlineBeatmapSetID) : BeatmapManager.QueryBeatmapSet(s => s.Hash == beatmap.Hash);

                if (databasedSet != null)
                {
                    // Use first beatmap available for current ruleset, else switch ruleset.
                    var first = databasedSet.Beatmaps.Find(b => b.Ruleset == ruleset.Value) ?? databasedSet.Beatmaps.First();

                    ruleset.Value = first.Ruleset;
                    Beatmap.Value = BeatmapManager.GetWorkingBeatmap(first);
                }
            }

            switch (screenStack.CurrentScreen)
            {
                case SongSelect _:
                    break;
                default:
                    // navigate to song select if we are not already there.

                    menuScreen.MakeCurrent();
                    menuScreen.LoadToSolo();
                    break;
            }

            setBeatmap();
        }

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

        protected void LoadScore(ScoreInfo score, bool silent)
        {
            if (silent)
                return;

            scoreLoad?.Cancel();

            if (menuScreen == null)
            {
                scoreLoad = Schedule(() => LoadScore(score, false));
                return;
            }

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

            if ((screenStack.CurrentScreen as IOsuScreen)?.AllowExternalScreenChange != true)
            {
                notifications.Post(new SimpleNotification
                {
                    Text = $"Click here to watch {databasedScoreInfo.User.Username} on {databasedScoreInfo.Beatmap}",
                    Activated = () =>
                    {
                        loadScore();
                        return true;
                    }
                });

                return;
            }

            loadScore();

            void loadScore()
            {
                if (!menuScreen.IsCurrentScreen())
                {
                    menuScreen.MakeCurrent();
                    this.Delay(500).Schedule(loadScore, out scoreLoad);
                    return;
                }

                ruleset.Value = databasedScoreInfo.Ruleset;

                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(databasedBeatmap);
                Beatmap.Value.Mods.Value = databasedScoreInfo.Mods;

                menuScreen.Push(new PlayerLoader(() => new ReplayPlayer(databasedScore)));
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            RavenLogger.Dispose();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // todo: all archive managers should be able to be looped here.
            SkinManager.PostNotification = n => notifications?.Post(n);
            SkinManager.GetStableStorage = GetStorageForStableInstall;

            BeatmapManager.PostNotification = n => notifications?.Post(n);
            BeatmapManager.GetStableStorage = GetStorageForStableInstall;

            BeatmapManager.PresentBeatmap = PresentBeatmap;

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
                        backgroundParallax = new ParallaxContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = backgroundStack = new BackgroundScreenStack { RelativeSizeAxes = Axes.Both },
                        },
                        screenStack = new ScreenStack { RelativeSizeAxes = Axes.Both },
                        logoContainer = new Container { RelativeSizeAxes = Axes.Both },
                    }
                },
                overlayContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                floatingOverlayContent = new Container { RelativeSizeAxes = Axes.Both, Depth = float.MinValue },
                idleTracker = new GameIdleTracker(6000)
            });

            dependencies.Cache(backgroundStack);

            screenStack.ScreenPushed += screenPushed;
            screenStack.ScreenExited += screenExited;

            loadComponentSingleFile(osuLogo, logoContainer.Add);

            loadComponentSingleFile(new Loader
            {
                RelativeSizeAxes = Axes.Both
            }, screenStack.Push);

            loadComponentSingleFile(Toolbar = new Toolbar
            {
                Depth = -5,
                OnHome = delegate
                {
                    CloseAllOverlays(false);
                    menuScreen?.MakeCurrent();
                },
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(volume = new VolumeOverlay(), floatingOverlayContent.Add);
            loadComponentSingleFile(onscreenDisplay = new OnScreenDisplay(), Add);

            loadComponentSingleFile(screenshotManager, Add);

            //overlay elements
            loadComponentSingleFile(direct = new DirectOverlay { Depth = -1 }, overlayContent.Add);
            loadComponentSingleFile(social = new SocialOverlay { Depth = -1 }, overlayContent.Add);
            loadComponentSingleFile(channelManager = new ChannelManager(), AddInternal);
            loadComponentSingleFile(chatOverlay = new ChatOverlay { Depth = -1 }, overlayContent.Add);
            loadComponentSingleFile(settings = new MainSettings
            {
                GetToolbarHeight = () => ToolbarOffset,
                Depth = -1
            }, floatingOverlayContent.Add);
            loadComponentSingleFile(userProfile = new UserProfileOverlay { Depth = -2 }, overlayContent.Add);
            loadComponentSingleFile(beatmapSetOverlay = new BeatmapSetOverlay { Depth = -3 }, overlayContent.Add);
            loadComponentSingleFile(musicController = new MusicController
            {
                Depth = -5,
                Position = new Vector2(0, Toolbar.HEIGHT),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(notifications = new NotificationOverlay
            {
                GetToolbarHeight = () => ToolbarOffset,
                Depth = -4,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(accountCreation = new AccountCreationOverlay
            {
                Depth = -6,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(dialogOverlay = new DialogOverlay
            {
                Depth = -7,
            }, floatingOverlayContent.Add);

            loadComponentSingleFile(externalLinkOpener = new ExternalLinkOpener
            {
                Depth = -8,
            }, floatingOverlayContent.Add);

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

            OverlayActivationMode.ValueChanged += v =>
            {
                if (v != OverlayActivation.All) CloseAllOverlays();
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
                        Icon = entry.Level == LogLevel.Important ? FontAwesome.fa_exclamation_circle : FontAwesome.fa_bomb,
                        Text = entry.Message + (entry.Exception != null && IsDeployedBuild ? "\n\nThis error has been automatically reported to the devs." : string.Empty),
                    }));
                }
                else if (recentLogCount == short_term_display_limit)
                {
                    Schedule(() => notifications.Post(new SimpleNotification
                    {
                        Icon = FontAwesome.fa_ellipsis_h,
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
        private int visibleOverlayCount;

        private void loadComponentSingleFile<T>(T d, Action<T> add)
            where T : Drawable
        {
            var focused = d as FocusedOverlayContainer;
            if (focused != null)
            {
                focused.StateChanged += s =>
                {
                    visibleOverlayCount += s == Visibility.Visible ? 1 : -1;
                    screenContainer.FadeColour(visibleOverlayCount > 0 ? OsuColour.Gray(0.5f) : Color4.White, 500, Easing.OutQuint);
                };
            }

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
                        await LoadComponentAsync(d, add);
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

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Audio.AddAdjustment(AdjustableProperty.Volume, inactiveVolumeAdjust);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            Audio.RemoveAdjustment(AdjustableProperty.Volume, inactiveVolumeAdjust);
        }

        public bool OnReleased(GlobalAction action) => false;

        private Container overlayContent;

        private Container floatingOverlayContent;

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

            // we only want to apply these restrictions when we are inside a screen stack.
            // the use case for not applying is in visual/unit tests.
            bool applyBeatmapRulesetRestrictions = !(screenStack.CurrentScreen as IOsuScreen)?.AllowBeatmapRulesetChange ?? false;

            ruleset.Disabled = applyBeatmapRulesetRestrictions;
            Beatmap.Disabled = applyBeatmapRulesetRestrictions;

            screenContainer.Padding = new MarginPadding { Top = ToolbarOffset };
            overlayContent.Padding = new MarginPadding { Top = ToolbarOffset };

            MenuCursorContainer.CanShowCursor = (screenStack.CurrentScreen as IOsuScreen)?.CursorVisible ?? false;
        }

        /// <summary>
        /// Sets <see cref="Beatmap"/> while ignoring any beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to set.</param>
        public void ForcefullySetBeatmap(WorkingBeatmap beatmap)
        {
            var beatmapDisabled = Beatmap.Disabled;

            Beatmap.Disabled = false;
            Beatmap.Value = beatmap;
            Beatmap.Disabled = beatmapDisabled;
        }

        /// <summary>
        /// Sets <see cref="Ruleset"/> while ignoring any ruleset restrictions.
        /// </summary>
        /// <param name="beatmap">The beatmap to set.</param>
        public void ForcefullySetRuleset(RulesetInfo ruleset)
        {
            var rulesetDisabled = this.ruleset.Disabled;

            this.ruleset.Disabled = false;
            this.ruleset.Value = ruleset;
            this.ruleset.Disabled = rulesetDisabled;
        }

        protected virtual void ScreenChanged(IScreen lastScreen, IScreen newScreen)
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
                backgroundParallax.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * newOsuScreen.BackgroundParallaxAmount;

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
