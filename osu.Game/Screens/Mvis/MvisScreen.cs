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
using osu.Game.Overlays.Music;
using osu.Framework.Audio.Track;
using osu.Game.Input.Bindings;
using osu.Framework.Input.Bindings;
using osu.Game.Configuration;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.Storyboard;
using osu.Game.Screens.Mvis.Objects;
using osu.Game.Input;
using osu.Framework.Timing;
using osu.Framework.Audio;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens
{
    public class MvisScreen : OsuScreen, IKeyBindingHandler<GlobalAction>
    {
        private const float DURATION = 750;

        private bool AllowCursor = false;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;
        public override bool AllowRateAdjustments => true;

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

        [Cached]
        private PlaylistOverlay playlist;

        private InputManager inputManager { get; set; }
        private Box dimBox;
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
        private Track Track;
        private BackgroundStoryBoardLoader sbLoader;
        private BgTrianglesContainer bgTriangles;
        private LoadingSpinner loadingSpinner;
        private BindableBool TrackRunning = new BindableBool();
        private readonly BindableBool SBEnableProxy = new BindableBool();
        private readonly BindableBool ShowParticles = new BindableBool();
        private readonly BindableFloat BgBlur = new BindableFloat();
        private readonly BindableFloat IdleBgDim = new BindableFloat();
        private readonly BindableFloat ContentAlpha = new BindableFloat();
        private readonly BindableDouble MusicSpeed = new BindableDouble();
        private readonly BindableBool AdjustFreq = new BindableBool();
        private readonly BindableBool NightcoreBeat = new BindableBool();
        private bool OverlaysHidden = false;
        private Drawable SBOverlayProxy;
        private FillFlowContainer bottomFillFlow;
        private BindableBool lockChanges = new BindableBool();
        private readonly IBindable<bool> IsIdle = new BindableBool();
        private BufferedBeatmapCover beatmapCover;
        private Container gameplayBackground;
        private Container particles;
        private NightcoreBeatContainer nightcoreBeatContainer;

        public float BottombarHeight => bottomBar.DrawHeight - bottomFillFlow.Y;

        public MvisScreen()
        {
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            InternalChildren = new Drawable[]
            {
                nightcoreBeatContainer = new NightcoreBeatContainer(),
                new ParallaxContainer
                {
                    Depth = float.MaxValue,
                    Name = "Beatmap Background Parallax",
                    RelativeSizeAxes = Axes.Both,
                    ParallaxAmount = 0.02f,
                    Child = beatmapCover = new BufferedBeatmapCover{ RelativeSizeAxes = Axes.Both }
                },
                new Container
                {
                    Name = "Overlay Container",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        bottomFillFlow = new FillFlowContainer
                        {
                            Name = "Bottom FillFlow",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                bottomBar = new BottomBar
                                {
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4Extensions.FromHex("#333")
                                        },
                                        new Container
                                        {
                                            Name = "Base Container",
                                            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Children = new Drawable[]
                                            {
                                                progressBarContainer = new HoverableProgressBarContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                                new Container
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
                                                                    Action = () => musicController?.PreviousTrack(),
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
                                                                    Action = () => musicController?.NextTrack(),
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
                                                                loopToggleButton = new BottomBarSwitchButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Undo,
                                                                    Action = () => Beatmap.Value.Track.Looping = loopToggleButton.ToggleableValue.Value,
                                                                    TooltipText = "单曲循环",
                                                                },
                                                                soloButton = new BottomBarButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.User,
                                                                    Action = () => InvokeSolo(),
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
                new MvisScreenContentContainer
                {
                    Depth = 1,
                    SetBottomPadding = () => BottombarHeight,
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
                                sbLoader = new BackgroundStoryBoardLoader(),
                                dimBox = new Box
                                {
                                    Name = "Dim Box",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0
                                },
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
                        loadingSpinner = new LoadingSpinner(true, true)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding(60)
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
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new MfMvisSection
                                            {
                                                Margin = new MarginPadding { Top = 0 },
                                            },
                                            playlist = new PlaylistOverlay
                                            {
                                                Padding = new MarginPadding{ Left = 5, Right = 10 },
                                                TakeFocusOnPopIn = false,
                                                RelativeSizeAxes = Axes.X,
                                            },
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
            config.BindWith(MfSetting.MvisEnableSBOverlayProxy, SBEnableProxy);
            config.BindWith(MfSetting.MvisShowParticles, ShowParticles);
            config.BindWith(MfSetting.MvisMusicSpeed, MusicSpeed);
            config.BindWith(MfSetting.MvisAdjustMusicWithFreq, AdjustFreq);
            config.BindWith(MfSetting.MvisEnableNightcoreBeat, NightcoreBeat);
        }

        protected override void LoadComplete()
        {
            BgBlur.BindValueChanged(v => beatmapCover.BlurTo(new Vector2(v.NewValue * 100), 300), true);
            ContentAlpha.BindValueChanged(_ => UpdateIdleVisuals());
            IdleBgDim.BindValueChanged(_ => UpdateIdleVisuals());

            Beatmap.BindValueChanged(v => updateComponentFromBeatmap(v.NewValue), true);

            MusicSpeed.BindValueChanged(_ => ApplyTrackAdjustments());
            AdjustFreq.BindValueChanged(_ => ApplyTrackAdjustments());
            NightcoreBeat.BindValueChanged(_ => ApplyTrackAdjustments(), true);

            IsIdle.BindValueChanged(v => { if (v.NewValue) TryHideOverlays(); });
            SBEnableProxy.BindValueChanged(v => UpdateStoryboardProxy(v.NewValue));
            ShowParticles.BindValueChanged(v => 
            {
                switch ( v.NewValue )
                {
                    case true:
                        particles.Child = new SpaceParticlesContainer();
                        break;
                    
                    case false:
                        particles.Clear();
                        break;
                }
            });

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
            });
            sbLoader.storyboardReplacesBackground.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case true:
                        beatmapCover.FadeColour(Color4.Black, 300);
                        break;

                    default:
                    case false:
                        beatmapCover.FadeColour(Color4.White, 300);
                        break;
                }
            });
            inputManager = GetContainingInputManager();
            dimBox.ScaleTo(1.1f);

            playlist.BeatmapSets.BindTo(musicController.BeatmapSets);
            playlist.Show();

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

        private void UpdateStoryboardProxy(bool AllowDisplayAboveGameplay)
        {
            //需要进一步优化，现在的逻辑仍然有些混乱
            if (!sbLoader.IsReady.Value || SBOverlayProxy == null) return;

            //重置proxy
            if (SBOverlayProxy != null) //如果SBOverlayProxy不是空，则从背景和面板容器中移除
            {
                sbLoader.Remove(SBOverlayProxy);
                gameplayContent.Remove(SBOverlayProxy);
            }

            switch (AllowDisplayAboveGameplay)
            {
                case true:
                    sbLoader.Remove(SBOverlayProxy);

                    gameplayContent.Add(SBOverlayProxy);
                    break;

                case false:
                    if (SBOverlayProxy != null)
                    {
                        gameplayContent.Remove(SBOverlayProxy);
                        sbLoader.Add(SBOverlayProxy);
                    }
                    break;
            }
        }

        private void SeekTo(double position) =>
            musicController?.SeekTo(position);

        protected override void Update()
        {
            base.Update();

            Track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;
            if (Track?.IsDummyDevice == false)
            {
                TrackRunning.Value = Track.IsRunning;
                progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
                progressBarContainer.progressBar.EndTime = Track.Length;
            }
            else
            {
                TrackRunning.Value = false;
                progressBarContainer.progressBar.CurrentTime = 0;
                progressBarContainer.progressBar.EndTime = 1;
            }
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            var track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : new TrackVirtual(Beatmap.Value.Track.Length);

            //各种背景层的动画
            Background?.Delay(250).Then().FadeOut(250);
            beatmapCover.FadeOut().Then().Delay(500).FadeIn(500);
            gameplayBackground.FadeOut().Then().Delay(250).FadeIn(500);

            //非背景层的动画
            gameplayContent.ScaleTo(0f).Then().ScaleTo(1f, DURATION, Easing.OutQuint);
            bottomFillFlow.MoveToY(bottomBar.Height + 30).Then().MoveToY(0, DURATION, Easing.OutQuint);

            loadingSpinner.Show();
        }

        public override bool OnExiting(IScreen next)
        {
            //重置Track
            Beatmap.Value.Track.Looping = false;
            Track = new TrackVirtual(Beatmap.Value.Track.Length);

            //停止beatmapLogo，取消故事版家在任务以及锁定变更
            beatmapLogo.Clock = new DecoupleableInterpolatingFramedClock();
            ((beatmapLogo.Clock as DecoupleableInterpolatingFramedClock)).Stop();
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
                    if (lockChanges.Value == true)
                        lockButton.FadeColour(Color4.Gray.Opacity(0.6f), 300);
                    else
                        lockButton.FadeColour(Color4.White, 300);
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
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

        private void InvokeSolo()
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

            game?.Toolbar.Hide();
            bottomFillFlow.MoveToY(bottomBar.Height, DURATION, Easing.OutQuint);
            bottomBar.FadeTo(0.01f, DURATION, Easing.OutQuint);

            AllowBack = false;
            AllowCursor = false;
            OverlaysHidden = true;
            UpdateIdleVisuals();
        }

        private void ShowOverlays(bool Locked = false)
        {
            if (lockChanges.Value) return;

            game?.Toolbar.Show();
            gameplayContent.FadeTo(1, DURATION, Easing.OutQuint);
            dimBox.FadeTo(0.6f, DURATION, Easing.OutQuint);

            bottomFillFlow.MoveToY(0, DURATION, Easing.OutQuint);
            bottomBar.FadeIn(DURATION, Easing.OutQuint);

            AllowCursor = true;
            AllowBack = true;
            OverlaysHidden = false;
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
                musicController?.Stop();
            else
                musicController?.Play();
        }

        private void UpdateIdleVisuals()
        {
            if (!OverlaysHidden)
                return;

            dimBox.FadeTo(IdleBgDim.Value, DURATION, Easing.OutQuint);
            gameplayContent.FadeTo(ContentAlpha.Value, DURATION, Easing.OutQuint);
        }

        private void ApplyTrackAdjustments()
        {
            var track = Beatmap.Value.Track;
            track.ResetSpeedAdjustments();
            track.Looping = loopToggleButton.ToggleableValue.Value;
            track.RestartPoint = 0;
            if ( AdjustFreq.Value )
                track.AddAdjustment(AdjustableProperty.Frequency, MusicSpeed);
            else
                track.AddAdjustment(AdjustableProperty.Tempo, MusicSpeed);

            if ( NightcoreBeat.Value )
                nightcoreBeatContainer.Show();
            else
                nightcoreBeatContainer.Hide();
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            this.Schedule( () => ApplyTrackAdjustments() );

            sbLoader.UpdateStoryBoardAsync(() =>
            {
                if (SBOverlayProxy != null)
                {
                    SBOverlayProxy.Hide();
                    SBOverlayProxy.Expire();
                }

                SBOverlayProxy = sbLoader.GetOverlayProxy();

                UpdateStoryboardProxy(SBEnableProxy.Value);
            });
        }
    }
}
