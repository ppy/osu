// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.UI;
using osu.Game.Screens.Mvis.UI.Objects;
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
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis;
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
using osu.Game.Users;

namespace osu.Game.Screens
{

    ///<summary>
    ///bug:
    ///故事版Overlay Proxy不消失(???)
    ///</summary>
    public class MvisScreen : OsuScreen, IKeyBindingHandler<GlobalAction>
    {
        private const float DURATION = 750;

        protected override UserActivity InitialActivity => new UserActivity.InMvis();
        public override bool HideOverlaysOnEnter => true;
        private bool AllowCursor = false;
        public override bool AllowBackButton => false;
        public override bool CursorVisible => AllowCursor;
        public override bool AllowRateAdjustments => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        private bool canReallyHide =>
            // don't hide if the user is hovering one of the panes, unless they are idle.
            (IsHovered || IsIdle.Value)
            // don't hide if the user is dragging a slider or otherwise.
            && inputManager?.DraggedDrawable == null
            // don't hide if a focused overlay is visible, like settings.
            && inputManager?.FocusedDrawable == null;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        private InputManager inputManager { get; set; }
        private BottomBar bottomBar;
        private Container gameplayContent;
        private SideBarSettingsPanel sidebarContainer;
        private BeatmapLogo beatmapLogo;
        private HoverableProgressBarContainer progressBarContainer;
        private BottomBarButton soloButton;
        private BottomBarButton prevButton;
        private BottomBarButton nextButton;
        private BottomBarSwitchButton loopToggleButton;
        private BottomBarSwitchButton sidebarToggleButton;
        private BottomBarOverlayLockSwitchButton lockButton;
        private BottomBarSwitchButton songProgressButton;
        private DrawableTrack Track => musicController.CurrentTrack;
        private BackgroundStoryBoardLoader sbLoader;
        private BgTrianglesContainer bgTriangles;
        private LoadingSpinner loadingSpinner;
        private BindableBool TrackRunning = new BindableBool();
        private readonly BindableBool ShowParticles = new BindableBool();
        private readonly BindableFloat BgBlur = new BindableFloat();
        private readonly BindableFloat IdleBgDim = new BindableFloat();
        private readonly BindableFloat ContentAlpha = new BindableFloat();
        private readonly BindableDouble MusicSpeed = new BindableDouble();
        private readonly BindableBool AdjustFreq = new BindableBool();
        private readonly BindableBool NightcoreBeat = new BindableBool();
        private bool OverlaysHidden = false;
        private FillFlowContainer bottomFillFlow;
        private BindableBool lockChanges = new BindableBool();
        private readonly IBindable<bool> IsIdle = new BindableBool();
        private Container gameplayBackground;
        private Container particles;
        private NightcoreBeatContainer nightcoreBeatContainer;
        private Container buttonsContainer;

        public MvisScreen()
        {
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            InternalChildren = new Drawable[]
            {
                nightcoreBeatContainer = new NightcoreBeatContainer
                {
                    Alpha = 0
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
                        bottomFillFlow = new FillFlowContainer
                        {
                            Name = "Bottom FillFlow",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Direction = FillDirection.Vertical,
                            LayoutDuration = DURATION,
                            LayoutEasing = Easing.OutQuint,
                            Children = new Drawable[]
                            {
                                bottomBar = new BottomBar
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
                                                                new BottomBarButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.ArrowLeft,
                                                                    Action = () => this.Exit(),
                                                                    TooltipText = "退出",
                                                                },
                                                                new BottomBarButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Regular.QuestionCircle,
                                                                    Action = () => game?.picture.UpdateImage("https://i0.hdslb.com/bfs/article/91cdfbdf623775b2bb9e93b6c0842cf5740ef912.png", true, false, "食用指南"),
                                                                    TooltipText = "Mvis播放器食用指南(需要网络连接)",
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
                                                                prevButton = new BottomBarButton()
                                                                {
                                                                    Size = new Vector2(50, 30),
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    ButtonIcon = FontAwesome.Solid.StepBackward,
                                                                    Action = () => musicController.PreviousTrack(),
                                                                    TooltipText = "上一首/从头开始",
                                                                },
                                                                songProgressButton = new BottomBarSwitchButton()
                                                                {
                                                                    TooltipText = "切换暂停",
                                                                    AutoSizeAxes = Axes.X,
                                                                    Action = () => TogglePause(),
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    NoIcon = true,
                                                                    ExtraDrawable = new BottomBarSongProgressInfo
                                                                    {
                                                                        AutoSizeAxes = Axes.Both,
                                                                        Anchor = Anchor.Centre,
                                                                        Origin = Anchor.Centre,
                                                                    }
                                                                },
                                                                nextButton = new BottomBarButton()
                                                                {
                                                                    Size = new Vector2(50, 30),
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    ButtonIcon = FontAwesome.Solid.StepForward,
                                                                    Action = () => musicController.NextTrack(),
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
                                                                new BottomBarButton
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Desktop,
                                                                    Action = () =>
                                                                    {
                                                                        //隐藏界面，锁定更改并隐藏锁定按钮
                                                                        lockChanges.Value = false;
                                                                        HideOverlays();
                                                                        if ( sidebarToggleButton.ToggleableValue.Value )
                                                                            sidebarToggleButton.Click();

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
                                                                loopToggleButton = new BottomBarSwitchButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Undo,
                                                                    Action = () => Track.Looping = loopToggleButton.ToggleableValue.Value,
                                                                    TooltipText = "单曲循环",
                                                                },
                                                                soloButton = new BottomBarButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.User,
                                                                    Action = () => PresentBeatmap(),
                                                                    TooltipText = "在选歌界面中查看",
                                                                },
                                                                sidebarToggleButton = new BottomBarSwitchButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Atom,
                                                                    Action = () => sidebarContainer.ToggleVisibility(),
                                                                    TooltipText = "侧边栏",
                                                                },
                                                            }
                                                        },
                                                    }
                                                },
                                                progressBarContainer = new HoverableProgressBarContainer
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
                                    Margin = new MarginPadding{ Bottom = 5 },
                                    Child = lockButton = new BottomBarOverlayLockSwitchButton
                                    {
                                        TooltipText = "切换悬浮锁",
                                        Action = () => UpdateLockButtonVisuals(),
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                    }
                                }
                            }
                        },
                    }
                },
                new Container
                {
                    Name = "Content Container",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding{Horizontal = 50},
                    Depth = 1,
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
                        },
                        sidebarContainer = new SideBarSettingsPanel
                        {
                            Name = "Sidebar Container",
                            RelativeSizeAxes = Axes.Y,
                            Width = 400,
                            Depth = -float.MaxValue,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Width = 400 + HORIZONTAL_OVERFLOW_PADDING,
                                    RelativeSizeAxes = Axes.Y,
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                },
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Spacing = new Vector2(20),
                                        Padding = new MarginPadding{ Top = 10, Left = 5, Right = 5 },
                                        Margin = new MarginPadding{Bottom = 50},
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new MfMvisSection
                                            {
                                                Margin = new MarginPadding { Top = 0 },
                                            },
                                            new SettingsButton()
                                            {
                                                Text = "歌曲选择",
                                                Action = () => this.Push(new MvisSongSelect())
                                            }
                                        }
                                    },
                                },
                            }
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config, IdleTracker idleTracker)
        {
            IsIdle.BindTo(idleTracker.IsIdle);
            config.BindWith(MfSetting.MvisBgBlur, BgBlur);
            config.BindWith(MfSetting.MvisIdleBgDim, IdleBgDim);
            config.BindWith(MfSetting.MvisContentAlpha, ContentAlpha);
            config.BindWith(MfSetting.MvisShowParticles, ShowParticles);
            config.BindWith(MfSetting.MvisMusicSpeed, MusicSpeed);
            config.BindWith(MfSetting.MvisAdjustMusicWithFreq, AdjustFreq);
            config.BindWith(MfSetting.MvisEnableNightcoreBeat, NightcoreBeat);
        }

        protected override void LoadComplete()
        {
            BgBlur.BindValueChanged(v => updateBackground(Beatmap.Value));
            ContentAlpha.BindValueChanged(_ => UpdateIdleVisuals());
            IdleBgDim.BindValueChanged(_ => UpdateIdleVisuals());
            lockChanges.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case true:
                        lockButton.FadeColour(Color4.Gray.Opacity(0.6f), 300);
                        break;

                    case false:
                        lockButton.FadeColour(Color4.White, 300);
                        break;
                }
            });

            Beatmap.BindValueChanged(OnBeatmapChanged, true);

            MusicSpeed.BindValueChanged(_ => ApplyTrackAdjustments());
            AdjustFreq.BindValueChanged(_ => ApplyTrackAdjustments());
            NightcoreBeat.BindValueChanged(_ => ApplyTrackAdjustments());

            IsIdle.BindValueChanged(v => { if (v.NewValue) TryHideOverlays(); });
            ShowParticles.BindValueChanged(v =>
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

            sbLoader.NeedToHideTriangles.BindValueChanged(UpdateBgTriangles, true);
            sbLoader.IsReady.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case true:
                        loadingSpinner.Hide();
                        break;

                    case false:
                        loadingSpinner.Show();
                        break;
                }
            }, true);

            sbLoader.storyboardReplacesBackground.BindValueChanged(_ => ApplyBackgroundBrightness());
            inputManager = GetContainingInputManager();

            progressBarContainer.progressBar.OnSeek = SeekTo;

            songProgressButton.ToggleableValue.BindTo(TrackRunning);

            ShowOverlays();

            base.LoadComplete();
        }

        ///<summary>
        ///是否需要隐藏背景三角形粒子
        ///</summary>
        private void UpdateBgTriangles(ValueChangedEvent<bool> value)
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

        private void SeekTo(double position)
        {
            musicController.SeekTo(position);
            sbLoader.Seek(position);
        }

        protected override void Update()
        {
            base.Update();

            TrackRunning.Value = Track.IsRunning;
            progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
            progressBarContainer.progressBar.EndTime = Track.Length;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            //各种背景层的动画
            gameplayBackground.FadeOut().Then().Delay(250).FadeIn(500);

            //非背景层的动画
            gameplayContent.ScaleTo(0f).Then().ScaleTo(1f, DURATION, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30).Then().MoveToY(0, DURATION, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            //重置Track
            Track.ResetSpeedAdjustments();
            Track.Looping = false;

            //停止beatmapLogo，取消故事版家在任务以及锁定变更
            beatmapLogo.StopResponseOnBeatmapChanges();
            sbLoader.CancelAllTasks();
            lockChanges.Value = true;

            //背景层的动画
            Background?.FadeIn(250);

            //非背景层的动画
            gameplayContent.ScaleTo(0, DURATION, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30, DURATION, Easing.OutQuint);

            this.FadeOut(500, Easing.OutQuint);
            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            Track.ResetSpeedAdjustments();

            //背景层的动画
            ApplyBackgroundBrightness(false, 1);
            Background?.FadeIn(250);

            //非背景层的动画
            gameplayContent.MoveToX(-DrawWidth, DURATION, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30, DURATION, Easing.OutQuint);

            if ( sidebarContainer.Alpha != 0 )
                sidebarContainer.Hide();

            this.FadeOut(DURATION, Easing.OutQuint);
            beatmapLogo.StopResponseOnBeatmapChanges();
            Beatmap.UnbindEvents();

            base.OnSuspending(next);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            this.FadeIn(DURATION);
            Track.ResetSpeedAdjustments();
            ApplyTrackAdjustments();
            updateBackground(Beatmap.Value);

            Beatmap.BindValueChanged(OnBeatmapChanged, true);
            beatmapLogo.ResponseOnBeatmapChanges();

            //背景层的动画
            gameplayBackground.FadeOut().Then().Delay(250).FadeIn(500);

            //非背景层的动画
            gameplayContent.MoveToX(0, DURATION, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30).Then().MoveToY(0, DURATION, Easing.OutQuint);

            if (sidebarToggleButton.ToggleableValue.Value)
                sidebarContainer.Show();
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
                    if (!OverlaysHidden)
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
                    TryShowOverlays();
                    return base.Handle(e);

                default:
                    return base.Handle(e);
            }
        }

        private void PresentBeatmap()
        {
            game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);
        }

        //当有弹窗或游戏失去焦点时要进行的动作
        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (lockButton.ToggleableValue.Value && OverlaysHidden)
                lockButton.Toggle();

            ShowOverlays();
            base.OnHoverLost(e);
        }

        private void UpdateLockButtonVisuals() =>
            lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);

        private void HideOverlays()
        {
            if (lockChanges.Value) return;

            buttonsContainer.MoveToY(bottomBar.Height, DURATION, Easing.OutQuint);
            progressBarContainer.MoveToY(5, DURATION, Easing.OutQuint);
            bottomBar.FadeOut(DURATION, Easing.OutQuint);

            AllowCursor = false;
            OverlaysHidden = true;
            UpdateIdleVisuals();
        }

        private void ShowOverlays(bool Locked = false)
        {
            if (lockChanges.Value) return;

            gameplayContent.FadeTo(1, DURATION, Easing.OutQuint);

            buttonsContainer.MoveToY(0, DURATION, Easing.OutQuint);
            progressBarContainer.MoveToY(0, DURATION, Easing.OutQuint);
            bottomBar.FadeIn(DURATION, Easing.OutQuint);

            AllowCursor = true;
            OverlaysHidden = false;

            ApplyBackgroundBrightness();
        }

        //下一步优化界面隐藏，显示逻辑
        private void TryHideOverlays()
        {
            if (!canReallyHide || !IsIdle.Value || !this.IsHovered
                 || bottomBar.Hovered.Value || lockButton.ToggleableValue.Value)
                return;

            HideOverlays();
        }

        private void TryShowOverlays()
        {
            //在有锁并且悬浮界面已隐藏或悬浮界面可见的情况下显示悬浮锁
            if ((lockButton.ToggleableValue.Value && OverlaysHidden) || !OverlaysHidden)
            {
                lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2500).FadeOut(500, Easing.OutQuint);
                return;
            }
            ShowOverlays();
        }

        private void TogglePause()
        {
            if (Track?.IsRunning == true)
                musicController.Stop();
            else
                musicController.Play();
        }

        private void UpdateIdleVisuals()
        {
            if (!OverlaysHidden)
                return;

            ApplyBackgroundBrightness(true, IdleBgDim.Value);
            gameplayContent.FadeTo(ContentAlpha.Value, DURATION, Easing.OutQuint);
        }

        private void ApplyTrackAdjustments()
        {
            Track.ResetSpeedAdjustments();
            Track.Looping = loopToggleButton.ToggleableValue.Value;
            Track.RestartPoint = 0;
            if (AdjustFreq.Value)
                Track.AddAdjustment(AdjustableProperty.Frequency, MusicSpeed);
            else
                Track.AddAdjustment(AdjustableProperty.Tempo, MusicSpeed);

            if (NightcoreBeat.Value)
                nightcoreBeatContainer.Show();
            else
                nightcoreBeatContainer.Hide();
        }

        private void updateBackground(WorkingBeatmap beatmap)
        {
            if ( Background == null || ! this.IsCurrentScreen() ) return;

            if ( Background is BackgroundScreenBeatmap backgroundScreenBeatmap )
            {
                backgroundScreenBeatmap.BlurAmount.Value = BgBlur.Value * 100;
                backgroundScreenBeatmap.Beatmap = beatmap;
            }

            ApplyBackgroundBrightness();
        }

        /// <summary>
        /// 将屏幕暗化应用到背景层
        /// </summary>
        /// <param name="auto">是否根据情况自动调整.</param>
        /// <param name="brightness">要调整的亮度.</param>
        private void ApplyBackgroundBrightness(bool auto = true, float brightness = 0)
        {
            if ( !this.IsCurrentScreen() ) return;

            if ( auto )
                Background?.FadeColour(sbLoader.storyboardReplacesBackground.Value? Color4.Black : OsuColour.Gray( OverlaysHidden ? IdleBgDim.Value : 0.6f ),
                                       DURATION,
                                       Easing.OutQuint );
            else
                Background?.FadeColour(OsuColour.Gray(brightness), DURATION, Easing.OutQuint);

            sbLoader?.FadeColour(OsuColour.Gray(auto ? (OverlaysHidden ? IdleBgDim.Value : 0.6f) : brightness), DURATION, Easing.OutQuint);
        }

        private void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            var beatmap = v.NewValue;

            this.Schedule(() =>
            {
                ApplyTrackAdjustments();
                updateBackground(beatmap);
            });

            sbLoader.UpdateStoryBoardAsync();
        }
    }
}
