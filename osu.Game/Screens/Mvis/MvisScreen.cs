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
using osu.Game.Screens.Mvis.Objects.Helpers;
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
using osu.Game.Screens.Play;
using osu.Game.Screens.Mvis.Storyboard;

namespace osu.Game.Screens
{
    /// <summary>
    /// 缝合怪 + 奥利给山警告
    /// </summary>
    public class MvisScreen : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>
    {
        private const float DURATION = 750;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);

        private bool AllowCursor = false;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;

        private bool canReallyHide =>
            // don't hide if the user is hovering one of the panes, unless they are idle.
            (IsHovered || idleTracker.IsIdle.Value)
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
        private MouseIdleTracker idleTracker;
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
        private BackgroundStoryBoardLoader bgSB;
        private BgTrianglesContainer bgTriangles;
        private LoadingSpinner loadingSpinner;
        private BottomBarSongProgressInfo progressInfo;
        private BindableBool TrackRunning = new BindableBool();
        private BindableBool SBEnableProxy = new BindableBool();
        private BindableFloat BgBlur = new BindableFloat();
        private BindableFloat IdleBgDim = new BindableFloat();
        private BindableFloat ContentAlpha = new BindableFloat();
        private bool OverlaysHidden = false;
        private Drawable SBOverlayProxy;
        private FillFlowContainer bottomFillFlow;
        private BindableBool lockChanges = new BindableBool();

        public float BottombarHeight => bottomBar.DrawHeight - bottomFillFlow.Y;

        public MvisScreen()
        {
            InternalChildren = new Drawable[]
            {
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
                                                                    ExtraDrawable = progressInfo = new BottomBarSongProgressInfo
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
                                                                    Action = () => ToggleSideBar(),
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
                                        Action = () => UpdateLockButton(),
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
                    GetBottombarHeight = () => BottombarHeight,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Name = "Background Elements Container",
                            Children = new Drawable[]
                            {
                                bgTriangles = new BgTrianglesContainer(),
                                bgSB = new BackgroundStoryBoardLoader(),
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
                            Name = "Mvis Gameplay Elements Container",
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new SpaceParticlesContainer(),
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
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                        },
                        loadingSpinner = new LoadingSpinner(true, true)
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding(60)
                        },
                    }
                },
                idleTracker = new MouseIdleTracker(3000),
            };
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            config.BindWith(MfSetting.MvisBgBlur, BgBlur);
            config.BindWith(MfSetting.MvisIdleBgDim, IdleBgDim);
            config.BindWith(MfSetting.MvisContentAlpha, ContentAlpha);
            config.BindWith(MfSetting.MvisEnableSBOverlayProxy, SBEnableProxy);
        }

        protected override void LoadComplete()
        {
            BgBlur.ValueChanged += _ => Background.BlurAmount.Value = BgBlur.Value * 100;
            ContentAlpha.BindValueChanged(_ => UpdateIdleVisuals());
            IdleBgDim.BindValueChanged(_ => UpdateIdleVisuals());

            Beatmap.ValueChanged += _ => updateComponentFromBeatmap(Beatmap.Value);

            idleTracker.IsIdle.BindValueChanged(_ => UpdateVisuals());
            idleTracker.ScreenHovered.BindValueChanged(_ => UpdateVisuals());

            SBEnableProxy.BindValueChanged(v => UpdateStoryboardProxy(v.NewValue), true);

            bgSB.NeedToHideTriangles.BindValueChanged(UpdateBgTriangles, true);
            bgSB.IsReady.BindValueChanged(v =>
            {
                switch (bgSB.IsReady.Value)
                {
                    case true:
                        UpdateStoryboardProxy(SBEnableProxy.Value, true);
                        loadingSpinner.Hide();
                        break;

                    case false:
                        loadingSpinner.Show();
                        break;
                }
            });
            bgSB.storyboardReplacesBackground.BindValueChanged(v => Background.StoryboardReplacesBackground.Value = v.NewValue);

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
            switch ( value.NewValue )
            {
                case true:
                    bgTriangles.Hide();
                    break;

                case false:
                    bgTriangles.Show();
                    break;
            }
        }

        private void UpdateStoryboardProxy(bool v, bool isBeatmapChanged = false)
        {
            //重置proxy
            if (SBOverlayProxy != null) //如果SBOverlayProxy不是空，则从背景和面板容器中移除
            {
                bgSB.Remove(SBOverlayProxy);
                gameplayContent.Remove(SBOverlayProxy);
                if ( isBeatmapChanged ) SBOverlayProxy = null;
            }

            if (SBOverlayProxy == null || SBOverlayProxy is Box) //如果SBOverlayProxy为空或为Box(没有故事版时的占位),则赋值
            {
                SBOverlayProxy = bgSB?.SBLayer?.dimmableSB?.OverlayLayerContainer?.CreateProxy() ?? new Box();
            }

            if ( !bgSB.IsReady.Value ) return;
            switch(v)
            {
                case true:
                    bgSB.Remove(SBOverlayProxy);

                    gameplayContent.Add(SBOverlayProxy);

                    SBOverlayProxy?.FadeIn(500);
                    break;

                case false:
                    if (SBOverlayProxy != null)
                    {
                        gameplayContent.Remove(SBOverlayProxy);
                        bgSB.Add(SBOverlayProxy);
                    }
                    break;
            }
        }

        private void SeekTo(double position)
        {
            musicController?.SeekTo(position);
            bgSB?.sbClock?.Seek(position);
        }

        protected override void Update()
        {
            base.Update();

            Track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;
            if (Track?.IsDummyDevice == false)
            {
                if ( Track.CurrentTime == 0 )
                    bgSB?.sbClock?.Seek(Track.CurrentTime);

                TrackRunning.Value = Track.IsRunning;
                progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
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
            track.RestartPoint = 0;

            loadingSpinner.Show();
            updateComponentFromBeatmap(Beatmap.Value, 500);
        }

        public override bool OnExiting(IScreen next)
        {
            Beatmap.Value.Track.Looping = false;
            Track = new TrackVirtual(Beatmap.Value.Track.Length);
            beatmapLogo.Exit();
            bgSB.CancelAllTasks();

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

        private void ToggleSideBar()
        {
            sidebarContainer.ToggleVisibility();
            this.Delay(DURATION).Schedule(() => UpdateVisuals());
        }

        private void UpdateVisuals()
        {
            var mouseIdle = idleTracker.IsIdle.Value;

            //如果有其他弹窗显示在播放器上方，解锁切换并显示界面
            if (!idleTracker.ScreenHovered.Value)
            {
                if (lockButton.ToggleableValue.Value && OverlaysHidden)
                    lockButton.Toggle();

                ShowOverlays();
                return;
            }

            switch (mouseIdle)
            {
                case true:
                    TryHideOverlays();
                    break;
            }
        }

        private void UpdateLockButton()
        {
            lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);
            idleTracker?.Reset();
            UpdateVisuals();
        }

        private void HideOverlays()
        {
            if ( lockChanges.Value ) return;

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
            if ( lockChanges.Value ) return;

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
            if (!canReallyHide || !idleTracker.IsIdle.Value || !idleTracker.ScreenHovered.Value
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
            {
                bgSB?.sbClock?.Stop();
                musicController?.Stop();
            }
            else
            {
                bgSB?.sbClock?.Start();
                musicController?.Play();
            }
        }

        private void UpdateIdleVisuals()
        {
            if (!OverlaysHidden)
                return;

            dimBox.FadeTo(IdleBgDim.Value, DURATION, Easing.OutQuint);
            gameplayContent.FadeTo(ContentAlpha.Value, DURATION, Easing.OutQuint);
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap, float displayDelay = 0)
        {
            Beatmap.Value.Track.Looping = loopToggleButton.ToggleableValue.Value;

            Background.Beatmap = beatmap;
            Background.BlurAmount.Value = BgBlur.Value * 100;

            this.Schedule(() =>
            {
                progressBarContainer.progressBar.EndTime = beatmap.Track.Length;
            });
            bgSB.UpdateStoryBoardAsync(displayDelay);
        }
    }
}
