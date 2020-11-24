// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.BottomBar.Buttons;
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Framework.Input.Bindings;
using osu.Game.Configuration;
using osu.Game.Screens.Mvis.BottomBar;
using osu.Game.Screens.Mvis.Storyboard;
using osu.Game.Input;
using osu.Framework.Audio;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Audio;
using osu.Game.Screens.Select;
using osu.Game.Overlays.Settings;
using osuTK.Input;
using osu.Game.Screens.Backgrounds;
using osu.Game.Graphics;
using osu.Framework;
using osu.Game.Screens.Mvis.Collections;
using osu.Game.Screens.Mvis.Collections.Interface;
using osu.Game.Users;
using osu.Game.Screens.Mvis.Objects;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Skinning;
using Sidebar = osu.Game.Screens.Mvis.SideBar.Sidebar;

namespace osu.Game.Screens.Mvis
{
    ///<summary>
    /// 音乐播放器
    ///</summary>
    public class MvisScreen : OsuScreen, IKeyBindingHandler<GlobalAction>
    {
        private const float duration = 750;

        public override bool HideOverlaysOnEnter => true;
        private bool allowCursor;
        public override bool AllowBackButton => false;
        public override bool CursorVisible => allowCursor;
        public override bool AllowRateAdjustments => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        private bool canReallyHide =>
            // don't hide if the user is hovering one of the panes, unless they are idle.
            (IsHovered || isIdle.Value)
            // don't hide if the user is dragging a slider or otherwise.
            && inputManager?.DraggedDrawable == null
            // don't hide if a focused overlay is visible, like settings.
            && inputManager?.FocusedDrawable == null;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        private InputManager inputManager { get; set; }
        private BottomBarContainer bottomBar;
        private Container gameplayContent;
        private Sidebar sidebar;
        private BeatmapLogo beatmapLogo;
        private SongProgressBar progressBar;
        private BottomBarButton soloButton;
        private BottomBarButton prevButton;
        private BottomBarButton nextButton;
        private BottomBarButton sidebarToggleButton;
        private BottomBarSwitchButton loopToggleButton;
        private BottomBarOverlayLockSwitchButton lockButton;
        private BottomBarSwitchButton songProgressButton;
        private BottomBarButton collectionButton;
        private DrawableTrack track => musicController.CurrentTrack;
        private BackgroundStoryBoardLoader sbLoader;
        private BgTrianglesContainer bgTriangles;
        private LoadingSpinner loadingSpinner;
        private readonly BindableBool trackRunning = new BindableBool();
        private readonly BindableBool showParticles = new BindableBool();
        private readonly BindableFloat bgBlur = new BindableFloat();
        private readonly BindableFloat idleBgDim = new BindableFloat();
        private readonly BindableFloat contentAlpha = new BindableFloat();
        private readonly BindableDouble musicSpeed = new BindableDouble();
        private readonly BindableBool adjustFreq = new BindableBool();
        private readonly BindableBool nightcoreBeat = new BindableBool();
        private readonly BindableBool playFromCollection = new BindableBool();
        private bool overlaysHidden;
        private FillFlowContainer bottomFillFlow;
        private readonly BindableBool lockChanges = new BindableBool();
        private readonly IBindable<bool> isIdle = new BindableBool();
        private Container gameplayBackground;
        private Container particles;
        private NightcoreBeatContainer nightcoreBeatContainer;
        private Container buttonsContainer;
        private CollectionHelper collectionHelper;
        private CollectionSelectPanel collectionPanel;
        private SidebarSettingsScrollContainer settingsScroll;
        private CustomColourProvider colourProvider;
        private DependencyContainer dependencies;
        private FullScreenSkinnableComponent skinnableForeground;
        private FullScreenSkinnableComponent skinnableBbBackground;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private readonly Bindable<UserActivity> activity = new Bindable<UserActivity>();

        public MvisScreen()
        {
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            Activity.BindTo(activity);
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config, IdleTracker idleTracker)
        {
            var iR = config.Get<float>(MfSetting.MvisInterfaceRed);
            var iG = config.Get<float>(MfSetting.MvisInterfaceGreen);
            var iB = config.Get<float>(MfSetting.MvisInterfaceBlue);
            dependencies.Cache(colourProvider = new CustomColourProvider(iR, iG, iB));
            dependencies.Cache(collectionHelper = new CollectionHelper());

            InternalChildren = new Drawable[]
            {
                colourProvider,
                collectionHelper,
                nightcoreBeatContainer = new NightcoreBeatContainer
                {
                    Alpha = 0
                },
                new Container
                {
                    Name = "Content Container",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 50 },
                    Masking = true,
                    Children = new Drawable[]
                    {
                        gameplayBackground = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Name = "Gameplay Background Elements Container",
                            Children = new Drawable[]
                            {
                                bgTriangles = new BgTrianglesContainer(),
                                sbLoader = new BackgroundStoryBoardLoader()
                            }
                        },
                        gameplayContent = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Name = "Gameplay Foreground Elements Container",
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                particles = new Container
                                {
                                    RelativeSizeAxes = Axes.Both
                                },
                                new ParallaxContainer
                                {
                                    ParallaxAmount = -0.0025f,
                                    Child = beatmapLogo = new BeatmapLogo
                                    {
                                        Anchor = Anchor.Centre,
                                    }
                                },
                            }
                        }
                    }
                },
                skinnableForeground = new FullScreenSkinnableComponent("MPlayer-foreground", confineMode: ConfineMode.ScaleToFill, defaultImplementation: _ => new PlaceHolder())
                {
                    Name = "前景图",
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Alpha = 0,
                    OverrideChildAnchor = true
                },
                new Container
                {
                    Name = "Overlay Container",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        loadingSpinner = new LoadingSpinner(true, true)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding(115)
                        },
                        sidebar = new Sidebar
                        {
                            Name = "Sidebar Container",
                            Padding = new MarginPadding { Right = HORIZONTAL_OVERFLOW_PADDING }
                        },
                        skinnableBbBackground = new FullScreenSkinnableComponent("MBottomBar-background",
                            confineMode: ConfineMode.ScaleToFill,
                            masking: true,
                            defaultImplementation: _ => new PlaceHolder())
                        {
                            Name = "底栏背景图",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 100,
                            ChildAnchor = Anchor.BottomCentre,
                            ChildOrigin = Anchor.BottomCentre,
                            Alpha = 0,
                            CentreComponent = false,
                            OverrideChildAnchor = true
                        },
                        bottomFillFlow = new FillFlowContainer
                        {
                            Name = "Bottom FillFlow",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Direction = FillDirection.Vertical,
                            LayoutDuration = duration,
                            LayoutEasing = Easing.OutQuint,
                            Children = new Drawable[]
                            {
                                bottomBar = new BottomBarContainer
                                {
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Name = "Base Container",
                                            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Children = new Drawable[]
                                            {
                                                buttonsContainer = new Container
                                                {
                                                    Name = "Buttons Container",
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Children = new Drawable[]
                                                    {
                                                        new FillFlowContainer
                                                        {
                                                            Name = "Left Buttons FillFlow",
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            AutoSizeAxes = Axes.Both,
                                                            Spacing = new Vector2(5),
                                                            Margin = new MarginPadding { Left = 5 },
                                                            Children = new Drawable[]
                                                            {
                                                                new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.ArrowLeft,
                                                                    Action = this.Exit,
                                                                    TooltipText = "退出",
                                                                },
                                                                new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Regular.QuestionCircle,
                                                                    Action = () => game?.OpenUrlExternally("https://matrix-feather.github.io/%E6%97%A5%E5%B8%B8/mfosu_mp_manual/"),
                                                                    TooltipText = "在浏览器中打开食用手册"
                                                                },
                                                            }
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            Name = "Centre Button FillFlow",
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            AutoSizeAxes = Axes.Both,
                                                            Spacing = new Vector2(5),
                                                            Children = new Drawable[]
                                                            {
                                                                prevButton = new NextPrevButton
                                                                {
                                                                    Size = new Vector2(50, 30),
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    ButtonIcon = FontAwesome.Solid.StepBackward,
                                                                    Action = prevTrack,
                                                                    TooltipText = "上一首/从头开始",
                                                                },
                                                                songProgressButton = new SongProgressButton
                                                                {
                                                                    TooltipText = "切换暂停",
                                                                    Action = togglePause,
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    NoIcon = true,
                                                                },
                                                                nextButton = new NextPrevButton
                                                                {
                                                                    Size = new Vector2(50, 30),
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    ButtonIcon = FontAwesome.Solid.StepForward,
                                                                    Action = nextTrack,
                                                                    TooltipText = "下一首",
                                                                },
                                                            }
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            Name = "Right Buttons FillFlow",
                                                            Anchor = Anchor.CentreRight,
                                                            Origin = Anchor.CentreRight,
                                                            AutoSizeAxes = Axes.Both,
                                                            Spacing = new Vector2(5),
                                                            Margin = new MarginPadding { Right = 5 },
                                                            Children = new Drawable[]
                                                            {
                                                                collectionButton = new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.List,
                                                                    TooltipText = "收藏夹选择",
                                                                    Action = () => updateSidebarState(collectionPanel)
                                                                },
                                                                new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Desktop,
                                                                    Action = () =>
                                                                    {
                                                                        //隐藏界面，锁定更改并隐藏锁定按钮
                                                                        lockChanges.Value = false;
                                                                        hideOverlays();

                                                                        updateSidebarState(null);

                                                                        //防止手机端无法退出桌面背景模式
                                                                        if (RuntimeInfo.IsDesktop)
                                                                        {
                                                                            lockChanges.Value = true;
                                                                            lockButton.ToggleableValue.Value = false;
                                                                        }
                                                                        else
                                                                        {
                                                                            lockChanges.Value = false;
                                                                            lockButton.ToggleableValue.Value = true;
                                                                        }
                                                                    },
                                                                    TooltipText = "桌面背景模式(切换强制锁定后移动鼠标即可恢复正常)"
                                                                },
                                                                loopToggleButton = new ToggleLoopButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Undo,
                                                                    Action = () => track.Looping = loopToggleButton.ToggleableValue.Value,
                                                                    TooltipText = "单曲循环",
                                                                },
                                                                soloButton = new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.User,
                                                                    Action = presentBeatmap,
                                                                    TooltipText = "在选歌界面中查看",
                                                                },
                                                                sidebarToggleButton = new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Cog,
                                                                    Action = () => updateSidebarState(settingsScroll),
                                                                    TooltipText = "播放器设置",
                                                                },
                                                            }
                                                        },
                                                    }
                                                },
                                                progressBar = new SongProgressBar
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                            }
                                        },
                                    }
                                },
                                new Container
                                {
                                    Name = "Lock Button Container",
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Margin = new MarginPadding { Bottom = 5 },
                                    Child = lockButton = new BottomBarOverlayLockSwitchButton
                                    {
                                        TooltipText = "切换悬浮锁",
                                        Action = updateLockButtonVisuals,
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                    }
                                }
                            }
                        }
                    }
                },
            };

            sidebar.Add(settingsScroll = new SidebarSettingsScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(20),
                    Padding = new MarginPadding { Top = 10, Left = 5, Right = 5 },
                    Margin = new MarginPadding { Bottom = 10 },
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new MfMvisSection
                        {
                            Margin = new MarginPadding { Top = -15 },
                            Padding = new MarginPadding(0)
                        },
                        new SettingsButton
                        {
                            Text = "歌曲选择",
                            Action = () => this.Push(new MvisSongSelect())
                        }
                    }
                },
            });
            sidebar.Add(collectionPanel = new CollectionSelectPanel());

            isIdle.BindTo(idleTracker.IsIdle);
            config.BindWith(MfSetting.MvisBgBlur, bgBlur);
            config.BindWith(MfSetting.MvisIdleBgDim, idleBgDim);
            config.BindWith(MfSetting.MvisContentAlpha, contentAlpha);
            config.BindWith(MfSetting.MvisShowParticles, showParticles);
            config.BindWith(MfSetting.MvisMusicSpeed, musicSpeed);
            config.BindWith(MfSetting.MvisAdjustMusicWithFreq, adjustFreq);
            config.BindWith(MfSetting.MvisEnableNightcoreBeat, nightcoreBeat);
            config.BindWith(MfSetting.MvisPlayFromCollection, playFromCollection);
        }

        protected override void LoadComplete()
        {
            bgBlur.BindValueChanged(v => updateBackground(Beatmap.Value));
            contentAlpha.BindValueChanged(_ => updateIdleVisuals());
            idleBgDim.BindValueChanged(_ => updateIdleVisuals());
            lockChanges.BindValueChanged(v =>
            {
                lockButton.FadeColour(v.NewValue
                    ? Color4.Gray.Opacity(0.6f)
                    : Color4.White, 300);
            });

            Beatmap.BindValueChanged(OnBeatmapChanged, true);

            musicSpeed.BindValueChanged(_ => applyTrackAdjustments());
            adjustFreq.BindValueChanged(_ => applyTrackAdjustments());
            nightcoreBeat.BindValueChanged(_ => applyTrackAdjustments());
            playFromCollection.BindValueChanged(v =>
            {
                //确保Beatmap.Value.Track.Completed中collectionHelper.PlayNextBeatmap只会触发一次
                Beatmap.Value.Track.Completed -= collectionHelper.PlayNextBeatmap;

                if (v.NewValue)
                {
                    Beatmap.Value.Track.Completed += collectionHelper.PlayNextBeatmap;
                    musicController.TrackAdjustTakenOver = true;
                }
                else
                {
                    musicController.TrackAdjustTakenOver = false;
                }
            }, true);

            //collectionHelper.CurrentCollection.BindValueChanged(_ =>
            //{
            //    if (PlayFromCollection.Value
            //            && !collectionHelper.currentCollectionContains(Beatmap.Value))
            //        collectionHelper.PlayFirstBeatmap();
            //});

            isIdle.BindValueChanged(v =>
            {
                if (v.NewValue) tryHideOverlays();
            });
            showParticles.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case true:
                        particles.Child = new SpaceParticlesContainer();
                        break;

                    case false:
                        particles.Clear();
                        break;
                }
            }, true);

            sbLoader.NeedToHideTriangles.BindValueChanged(updateBgTriangles, true);
            sbLoader.State.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case StoryboardState.Success:
                    case StoryboardState.NotLoaded:
                        loadingSpinner.Hide();
                        break;

                    case StoryboardState.Loading:
                        loadingSpinner.Show();
                        loadingSpinner.FadeColour(Color4.White, 300);
                        break;

                    case StoryboardState.Waiting:
                        loadingSpinner.Show();
                        loadingSpinner.FadeColour(Color4.Gray);
                        break;

                    case StoryboardState.Failed:
                        loadingSpinner.Show();
                        loadingSpinner.FadeColour(Colour4.Red, 300);
                        break;
                }
            }, true);

            sbLoader.StoryboardReplacesBackground.BindValueChanged(_ => applyBackgroundBrightness());
            inputManager = GetContainingInputManager();

            progressBar.OnSeek = seekTo;

            songProgressButton.ToggleableValue.BindTo(trackRunning);

            showOverlays();

            base.LoadComplete();
        }

        ///<summary>
        ///是否需要隐藏背景三角形粒子
        ///</summary>
        private void updateBgTriangles(ValueChangedEvent<bool> value)
        {
            switch (value.NewValue)
            {
                case true:
                    bgTriangles.Hide();
                    break;

                case false:
                    bgTriangles.Show();
                    break;
            }
        }

        private void updateSidebarState(Drawable d)
        {
            if (d == null) sidebar.Hide();
            if (!(d is ISidebarContent)) return;

            var sc = (ISidebarContent)d;

            //如果sc是上一个显示(mvis)或sc是侧边栏的当前显示并且侧边栏未隐藏
            if (sc == sidebar.CurrentDisplay.Value && !sidebar.Hiding)
            {
                sidebar.Hide();
                return;
            }

            sidebar.ShowComponent(d);
        }

        private void seekTo(double position)
        {
            musicController.SeekTo(position);
            sbLoader.Seek(position);
        }

        protected override void Update()
        {
            base.Update();

            trackRunning.Value = track.IsRunning;
            progressBar.CurrentTime = track.CurrentTime;
            progressBar.EndTime = track.Length;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            //各种背景层的动画
            gameplayBackground.FadeOut().Then().Delay(250).FadeIn(500);

            //非背景层的动画
            gameplayContent.ScaleTo(0f).Then().ScaleTo(1f, duration, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30).Then().MoveToY(0, duration, Easing.OutQuint);
            skinnableForeground.FadeIn(duration, Easing.OutQuint);

            playFromCollection.TriggerChange();
        }

        public override bool OnExiting(IScreen next)
        {
            //重置Track
            track.ResetSpeedAdjustments();
            track.Looping = false;
            musicController.TrackAdjustTakenOver = false;

            //停止beatmapLogo，取消故事版家在任务以及锁定变更
            beatmapLogo.StopResponseOnBeatmapChanges();
            lockChanges.Value = true;

            //背景层的动画
            Background?.FadeIn(250);

            //非背景层的动画
            gameplayContent.ScaleTo(0, duration, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30, duration, Easing.OutQuint);

            this.FadeOut(500, Easing.OutQuint);
            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            track.ResetSpeedAdjustments();
            playFromCollection.TriggerChange();

            //背景层的动画
            applyBackgroundBrightness(false, 1);
            Background?.FadeIn(250);

            //非背景层的动画
            gameplayContent.MoveToX(-DrawWidth, duration, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30, duration, Easing.OutQuint);

            this.FadeOut(duration, Easing.OutQuint);
            beatmapLogo.StopResponseOnBeatmapChanges();
            Beatmap.UnbindEvents();

            base.OnSuspending(next);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            musicController.TrackAdjustTakenOver = true;
            collectionHelper.UpdateBeatmaps();
            collectionPanel.RefreshCollectionList();

            this.FadeIn(duration);
            track.ResetSpeedAdjustments();
            applyTrackAdjustments();
            updateBackground(Beatmap.Value);

            Beatmap.BindValueChanged(OnBeatmapChanged, true);
            beatmapLogo.ResponseOnBeatmapChanges();

            //背景层的动画
            gameplayBackground.FadeOut().Then().Delay(250).FadeIn(500);

            //非背景层的动画
            gameplayContent.MoveToX(0, duration, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30).Then().MoveToY(0, duration, Easing.OutQuint);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.MvisMusicPrev:
                    prevButton.Click();
                    return true;

                case GlobalAction.MvisMusicNext:
                    nextButton.Click();
                    return true;

                case GlobalAction.MvisTogglePause:
                    songProgressButton.Click();
                    return true;

                case GlobalAction.MvisTogglePlayList:
                    sidebarToggleButton.Click();
                    return true;

                case GlobalAction.MvisOpenInSongSelect:
                    soloButton.Click();
                    return true;

                case GlobalAction.MvisToggleOverlayLock:
                    lockButton.Click();
                    return true;

                case GlobalAction.MvisToggleTrackLoop:
                    loopToggleButton.Click();
                    return true;

                case GlobalAction.MvisForceLockOverlayChanges:
                    lockChanges.Toggle();
                    return true;

                case GlobalAction.MvisSelectCollection:
                    collectionButton.Click();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (!overlaysHidden)
                        this.Exit();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case MouseMoveEvent _:
                    tryShowOverlays();
                    return base.Handle(e);

                default:
                    return base.Handle(e);
            }
        }

        private void presentBeatmap() =>
            game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);

        //当有弹窗或游戏失去焦点时要进行的动作
        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (lockButton.ToggleableValue.Value && overlaysHidden)
                lockButton.Toggle();

            showOverlays();
            base.OnHoverLost(e);
        }

        private void updateLockButtonVisuals() =>
            lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);

        private void hideOverlays()
        {
            if (lockChanges.Value) return;

            skinnableBbBackground.MoveToY(bottomBar.Height, duration, Easing.OutQuint)
                                 .FadeOut(duration, Easing.OutQuint);
            buttonsContainer.MoveToY(bottomBar.Height, duration, Easing.OutQuint);
            progressBar.MoveToY(5, duration, Easing.OutQuint);
            bottomBar.FadeOut(duration, Easing.OutQuint);

            allowCursor = false;
            overlaysHidden = true;
            updateIdleVisuals();
        }

        private void showOverlays()
        {
            if (lockChanges.Value) return;

            gameplayContent.FadeTo(1, duration, Easing.OutQuint);

            skinnableBbBackground.MoveToY(0, duration, Easing.OutQuint)
                                 .FadeIn(duration, Easing.OutQuint);
            buttonsContainer.MoveToY(0, duration, Easing.OutQuint);
            progressBar.MoveToY(0, duration, Easing.OutQuint);
            bottomBar.FadeIn(duration, Easing.OutQuint);

            allowCursor = true;
            overlaysHidden = false;

            applyBackgroundBrightness();
        }

        //下一步优化界面隐藏，显示逻辑
        private void tryHideOverlays()
        {
            if (!canReallyHide || !isIdle.Value || !IsHovered
                || bottomBar.Hovered.Value || lockButton.ToggleableValue.Value)
                return;

            hideOverlays();
        }

        private void tryShowOverlays()
        {
            //在有锁并且悬浮界面已隐藏或悬浮界面可见的情况下显示悬浮锁
            if ((lockButton.ToggleableValue.Value && overlaysHidden) || !overlaysHidden)
            {
                lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2500).FadeOut(500, Easing.OutQuint);
                return;
            }

            showOverlays();
        }

        private void togglePause()
        {
            if (track?.IsRunning == true)
                musicController.Stop();
            else
                musicController.Play();
        }

        private void prevTrack()
        {
            if (playFromCollection.Value)
                collectionHelper.PrevTrack();
            else
                musicController.PreviousTrack();
        }

        private void nextTrack()
        {
            if (playFromCollection.Value)
                collectionHelper.NextTrack();
            else
                musicController.NextTrack();
        }

        private void updateIdleVisuals()
        {
            if (!overlaysHidden)
                return;

            applyBackgroundBrightness(true, idleBgDim.Value);
            gameplayContent.FadeTo(contentAlpha.Value, duration, Easing.OutQuint);
        }

        private void applyTrackAdjustments()
        {
            track.ResetSpeedAdjustments();
            track.Looping = loopToggleButton.ToggleableValue.Value;
            track.RestartPoint = 0;
            track.AddAdjustment(adjustFreq.Value ? AdjustableProperty.Frequency : AdjustableProperty.Tempo, musicSpeed);

            if (nightcoreBeat.Value)
                nightcoreBeatContainer.Show();
            else
                nightcoreBeatContainer.Hide();
        }

        private void updateBackground(WorkingBeatmap beatmap)
        {
            if (Background == null || !this.IsCurrentScreen()) return;

            if (Background is BackgroundScreenBeatmap backgroundScreenBeatmap)
            {
                backgroundScreenBeatmap.BlurAmount.Value = bgBlur.Value * 100;
                backgroundScreenBeatmap.Beatmap = beatmap;
            }

            applyBackgroundBrightness();
        }

        /// <summary>
        /// 将屏幕暗化应用到背景层
        /// </summary>
        /// <param name="auto">是否根据情况自动调整.</param>
        /// <param name="brightness">要调整的亮度.</param>
        private void applyBackgroundBrightness(bool auto = true, float brightness = 0)
        {
            if (!this.IsCurrentScreen()) return;

            if (auto)
            {
                Background?.FadeColour(
                    sbLoader.StoryboardReplacesBackground.Value ? Color4.Black : OsuColour.Gray(overlaysHidden ? idleBgDim.Value : 0.6f),
                    duration,
                    Easing.OutQuint);
            }
            else
                Background?.FadeColour(OsuColour.Gray(brightness), duration, Easing.OutQuint);

            sbLoader?.FadeColour(OsuColour.Gray(auto ? (overlaysHidden ? idleBgDim.Value : 0.6f) : brightness), duration, Easing.OutQuint);
        }

        private WorkingBeatmap prevBeatmap;

        private void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            var beatmap = v.NewValue;
            playFromCollection.TriggerChange();

            Schedule(() =>
            {
                applyTrackAdjustments();
                updateBackground(beatmap);
            });

            if (beatmap != prevBeatmap)
                sbLoader.UpdateStoryBoardAsync(beatmap);

            activity.Value = new UserActivity.InMvis(beatmap.BeatmapInfo);
            prevBeatmap = beatmap;
        }
    }
}
