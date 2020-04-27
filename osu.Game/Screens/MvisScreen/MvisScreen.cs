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
using osu.Game.Screens.Play;
using osu.Game.Screens.Mvis;

namespace osu.Game.Screens
{
    /// <summary>
    /// 缝合怪 + 奥利给山警告
    /// </summary>
    public class MvisScreen : OsuScreen, IKeyBindingHandler<GlobalAction>
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
        private ScheduledDelegate scheduledHideOverlays;
        private ScheduledDelegate scheduledShowOverlays;
        private Box bgBox;
        private Container sbContainer;
        ClockContainer sbClock;
        private BottomBar bottomBar;
        private SideBarSettingsPanel sidebarContainer;
        private BeatmapLogo beatmapLogo;
        private HoverCheckContainer hoverCheckContainer;
        private HoverableProgressBarContainer progressBarContainer;
        private ToggleableButton loopToggleButton;
        private ToggleableButton sidebarToggleButton;
        private ToggleableOverlayLockButton lockButton;
        private Track Track;
        private Bindable<float> BgBlur = new Bindable<float>();
        private bool OverlaysHidden = false;

        public MvisScreen()
        {
            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0
                },
                new Container
                {
                    Name = "Content Container",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        sbContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new SpaceParticlesContainer(),
                        new ParallaxContainer
                        {
                            ParallaxAmount = -0.0025f,
                            Child = beatmapLogo = new BeatmapLogo
                            {
                                Anchor = Anchor.Centre,
                            }
                        },
                        hoverCheckContainer = new HoverCheckContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                new Container
                {
                    Name = "Overlay Container",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
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
                                new FillFlowContainer
                                {
                                    Spacing = new Vector2(10),
                                    Padding = new MarginPadding{ Top = 10, Left = 5, Right = 5 },
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new MvisSettings(),
                                        playlist = new PlaylistOverlay
                                        {
                                            TakeFocusOnPopIn = false,
                                            RelativeSizeAxes = Axes.X,
                                        },
                                    }
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            Name = "Bottom FillFlow",
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),
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
                                                                    Action = () => musicController?.TogglePause(),
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
                                lockButton = new ToggleableOverlayLockButton
                                {
                                    TooltipText = "开启悬浮锁",
                                    Action = () => UpdateLockButton(),
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                }
                            }
                        },
                    }
                },
                idleTracker = new MouseIdleTracker(2000),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.MvisBgBlur, BgBlur);

            BgBlur.ValueChanged += _ => UpdateBgBlur();
        }

        protected override void LoadComplete()
        {
            Beatmap.ValueChanged += _ => updateComponentFromBeatmap(Beatmap.Value);
            idleTracker.IsIdle.ValueChanged += _ => UpdateVisuals();
            hoverCheckContainer.ScreenHovered.ValueChanged += _ => UpdateVisuals();

            inputManager = GetContainingInputManager();
            bgBox.ScaleTo(1.1f);

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

            updateComponentFromBeatmap(Beatmap.Value);
        }

        public override bool OnExiting(IScreen next)
        {
            Track = new TrackVirtual(Beatmap.Value.Track.Length);
            beatmapLogo.Exit();

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
                    musicController.TogglePause();
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

        private void InvokeSolo()
        {
            game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);
        }

        private void ToggleSideBar()
        {
            sidebarContainer.ToggleVisibility();
            this.Delay(DURATION).Schedule( () => UpdateVisuals());
        }

        private void UpdateVisuals()
        {
            var mouseIdle = idleTracker.IsIdle.Value;

            //如果有其他弹窗显示在播放器上方，解锁切换并显示界面
            if ( !hoverCheckContainer.ScreenHovered.Value )
            {
                if ( lockButton.ToggleableValue.Value && OverlaysHidden )
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

        private void UpdateLockButton()
        {
            lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);
            UpdateVisuals();
        }

        private void HideOverlays()
        {
            game?.Toolbar.Hide();
            bgBox.FadeTo(0.3f, DURATION, Easing.OutQuint);
            bottomBar.ResizeHeightTo(0, DURATION, Easing.OutQuint)
                     .FadeOut(DURATION, Easing.OutQuint);
            AllowBack = false;
            AllowCursor = false;
            OverlaysHidden = true;
        }

        private void ShowOverlays(bool Locked = false)
        {
            game?.Toolbar.Show();
            bgBox.FadeTo(0.6f, DURATION, Easing.OutQuint);
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
            if ( !idleTracker.IsIdle.Value || !hoverCheckContainer.ScreenHovered.Value
                 || bottomBar.Hovered.Value || lockButton.ToggleableValue.Value )
                return;

            HideOverlays();
        }

        private void RunShowOverlays()
        {
            //在有锁并且悬浮界面已隐藏或悬浮界面可见的情况下显示悬浮锁
            if ( (lockButton.ToggleableValue.Value && OverlaysHidden) || !OverlaysHidden )
            {
                lockButton.FadeIn(500, Easing.OutQuint).Then().Delay(2500).FadeOut(500, Easing.OutQuint);
                return;
            }
            ShowOverlays();
        }

        private void TryHideOverlays()
        {
            if ( !canReallyHide || bottomBar.Hovered.Value)
                return;

            try
            {
                scheduledHideOverlays = Scheduler.AddDelayed(() =>
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
                scheduledShowOverlays = Scheduler.AddDelayed(() => 
                {
                    RunShowOverlays();
                }, 0);
            }
            finally
            {
            }
        }

        private void UpdateBgBlur()
        {
            if (Background is BackgroundScreenBeatmap backgroundBeatmap)
            {
                backgroundBeatmap.BlurAmount.Value = BgBlur.Value * 100;
            }
        }

        private void UpdateStoryBoardSource()
        {
            sbClock?.Stop();
            foreach (var s in sbContainer)
            {
                s.Hide();
                s.Expire();
            }
            
            sbContainer.Add(
                sbClock = new ClockContainer(Beatmap.Value, 0)
                {
                    Child = new DimmableStoryboard(Beatmap.Value.Storyboard) { RelativeSizeAxes = Axes.Both }
                });

            sbClock.Start();
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            Beatmap.Value.Track.Looping = loopToggleButton.ToggleableValue.Value;

            if (Background is BackgroundScreenBeatmap backgroundBeatmap)
            {
                backgroundBeatmap.Beatmap = beatmap;
                backgroundBeatmap.BlurAmount.Value = BgBlur.Value * 100;
            }
            UpdateStoryBoardSource();
        }
    }
}
