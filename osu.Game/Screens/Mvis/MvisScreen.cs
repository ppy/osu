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
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Mvis.UI.Objects;
using osu.Game.Screens.Mvis.Buttons;
using osu.Game.Screens.Mvis.Objects.Helpers;
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Overlays.Music;
using osu.Framework.Audio.Track;
using osu.Game.Input.Bindings;
using osu.Framework.Input.Bindings;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings.Sections.General;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Play;

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
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

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
        private HoverCheckContainer hoverCheckContainer;
        private HoverableProgressBarContainer progressBarContainer;
        private ToggleableButton loopToggleButton;
        private ToggleableButton sidebarToggleButton;
        private ToggleableOverlayLockButton lockButton;
        private Track Track;
        private BackgroundStoryBoard bgSB;
        private LoadingSpinner loadingSpinner;
        private Bindable<float> BgBlur = new Bindable<float>();
        private Bindable<float> IdleBgDim = new Bindable<float>();
        private Bindable<float> ContentAlpha = new Bindable<float>();
        private bool OverlaysHidden = false;
        public float BottombarHeight => bottomBar.Position.Y + bottomBar.DrawHeight;

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
                        new FillFlowContainer
                        {
                            Name = "Bottom FillFlow",
                            RelativeSizeAxes = Axes.Both,
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
                                                                new MusicControlButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.StepBackward,
                                                                    Action = () => musicController?.PreviousTrack(),
                                                                    TooltipText = "上一首/从头开始",
                                                                },
                                                                new MusicControlButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Music,
                                                                    Action = () => TogglePause(),
                                                                    TooltipText = "切换暂停",
                                                                },
                                                                new MusicControlButton()
                                                                {
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
                                                                loopToggleButton = new ToggleableButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.Undo,
                                                                    Action = () => Beatmap.Value.Track.Looping = loopToggleButton.ToggleableValue.Value,
                                                                    TooltipText = "单曲循环",
                                                                },
                                                                new BottomBarButton()
                                                                {
                                                                    ButtonIcon = FontAwesome.Solid.User,
                                                                    Action = () => InvokeSolo(),
                                                                    TooltipText = "在选歌界面中查看",
                                                                },
                                                                sidebarToggleButton = new ToggleableButton()
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
                                    Child = lockButton = new ToggleableOverlayLockButton
                                    {
                                        TooltipText = "开启悬浮锁",
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
                    Children = new Drawable[]
                    {
                        new Container()
                        {
                            RelativeSizeAxes = Axes.Both,
                            Name = "Background Elements Container",
                            Children = new Drawable[]
                            {
                                bgSB = new BackgroundStoryBoard(),
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
                            Name = "Mvis Gameplay Item Container",
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
                                        Spacing = new Vector2(10),
                                        Padding = new MarginPadding{ Top = 10, Left = 5, Right = 5 },
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new MvisSettings(),
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
                idleTracker = new MouseIdleTracker(2000),
                hoverCheckContainer = new HoverCheckContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.MvisBgBlur, BgBlur);
            config.BindWith(OsuSetting.MvisIdleBgDim, IdleBgDim);
            config.BindWith(OsuSetting.MvisContentAlpha, ContentAlpha);
        }

        protected override void LoadComplete()
        {
            BgBlur.ValueChanged += _ => UpdateBgBlur();
            ContentAlpha.ValueChanged += _ => UpdateIdleVisuals();
            IdleBgDim.ValueChanged += _ => UpdateIdleVisuals();
            Beatmap.ValueChanged += _ => updateComponentFromBeatmap(Beatmap.Value);
            idleTracker.IsIdle.ValueChanged += _ => UpdateVisuals();
            hoverCheckContainer.ScreenHovered.ValueChanged += _ => UpdateVisuals();
            bgSB.IsReady.ValueChanged += _ =>
            {
                switch (bgSB.IsReady.Value)
                {
                    case true:
                        loadingSpinner.Hide();
                        break;

                    case false:
                        loadingSpinner.Show();
                        break;
                }
            };
            bgSB.storyboardReplacesBackground.ValueChanged += _ => 
            {
                Background.StoryboardReplacesBackground.Value = bgSB.storyboardReplacesBackground.Value;
            };

            inputManager = GetContainingInputManager();
            dimBox.ScaleTo(1.1f);

            playlist.BeatmapSets.BindTo(musicController.BeatmapSets);
            playlist.Show();

            ShowOverlays();

            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();

            Track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;
            if (Track?.IsDummyDevice == false)
            {
                progressBarContainer.progressBar.EndTime = Track.Length;
                progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
            }
            else
            {
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
                    musicController.PreviousTrack();
                    return true;

                case GlobalAction.MvisMusicNext:
                    musicController.NextTrack();
                    return true;

                case GlobalAction.MvisTogglePause:
                    TogglePause();
                    return true;

                case GlobalAction.MvisTogglePlayList:
                    sidebarToggleButton.Click();
                    return true;

                case GlobalAction.MvisOpenInSongSelect:
                    InvokeSolo();
                    return true;

                case GlobalAction.MvisToggleOverlayLock:
                    lockButton.Click();
                    return true;

                case GlobalAction.MvisToggleTrackLoop:
                    loopToggleButton.Click();
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
            if (!hoverCheckContainer.ScreenHovered.Value)
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
            UpdateVisuals();
        }

        private void HideOverlays()
        {
            game?.Toolbar.Hide();
            bottomBar.ResizeHeightTo(0, DURATION, Easing.OutQuint)
                     .FadeOut(DURATION, Easing.OutQuint);
            AllowBack = false;
            AllowCursor = false;
            OverlaysHidden = true;
            UpdateIdleVisuals();
        }

        private void ShowOverlays(bool Locked = false)
        {
            game?.Toolbar.Show();
            gameplayContent.FadeTo(1, DURATION, Easing.OutQuint);
            dimBox.FadeTo(0.6f, DURATION, Easing.OutQuint);
            bottomBar.ResizeHeightTo(BOTTOMPANEL_SIZE.Y, DURATION, Easing.OutQuint)
                     .FadeIn(DURATION, Easing.OutQuint);
            AllowCursor = true;
            AllowBack = true;
            OverlaysHidden = false;
        }

        /// <summary>
        /// 因为未知原因, <see cref="TryHideOverlays"/>调用的<see cref="HideOverlays"/>无法被<see cref="ShowOverlays"/>中断
        /// 因此将相关功能独立出来作为单独的函数用来调用
        /// </summary>
        private void RunHideOverlays()
        {
            if (!idleTracker.IsIdle.Value || !hoverCheckContainer.ScreenHovered.Value
                 || bottomBar.Hovered.Value || lockButton.ToggleableValue.Value)
                return;

            HideOverlays();
        }

        private void RunShowOverlays()
        {
            //在有锁并且悬浮界面已隐藏或悬浮界面可见的情况下显示悬浮锁
            if ((lockButton.ToggleableValue.Value && OverlaysHidden) || !OverlaysHidden)
            {
                lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2500).FadeOut(500, Easing.OutQuint);
                return;
            }
            ShowOverlays();
        }

        private void TryHideOverlays()
        {
            if (!canReallyHide || bottomBar.Hovered.Value)
                return;

            try
            {
                Scheduler.AddDelayed(() =>
                {
                    RunHideOverlays();
                }, 1000);
            }
            finally
            {
            }
        }

        private void TryShowOverlays()
        {
            try
            {
                Scheduler.AddDelayed(() =>
                {
                    RunShowOverlays();
                }, 0);
            }
            finally
            {
            }
        }

        private void TogglePause()
        {
            if ( Track?.IsRunning == true )
            {
                bgSB?.sbClock?.Stop();
                musicController.Stop();
            }
            else
            {
                bgSB?.sbClock?.Start();
                musicController.Play();
            }
        }

        private void UpdateBgBlur()
        {
            Background.BlurAmount.Value = BgBlur.Value * 100;
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

            bgSB.UpdateStoryBoardAsync( displayDelay );
        }
    }
}
