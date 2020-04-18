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
using osuTK.Input;
using osu.Game.Overlays.Music;

namespace osu.Game.Screens
{
    /// <summary>
    /// 缝合怪 + 奥利给山警告
    /// </summary>
    public class MvisScreen : OsuScreen
    {
        private const float DURATION = 750;
        protected const float BACKGROUND_BLUR = 20;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);

        private bool AllowCursor = false;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);


        private bool canReallyHide =>
            // don't hide if the user is hovering one of the panes, unless they are idle.
            (IsHovered || idleTracker.IsIdle.Value)
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

        private Box bgBox;
        private BottomBar bottomBar;
        private bool ScheduleDone = false;
        private ScheduledDelegate scheduledHideBars;
        Container buttons;
        ParallaxContainer beatmapParallax;
        HoverCheckContainer hoverCheckContainer;
        HoverableProgressBarContainer progressBarContainer;

        public MvisScreen()
        {
            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.3f
                },
                new SpaceParticlesContainer(),
                beatmapParallax = new ParallaxContainer
                {
                    ParallaxAmount = -0.0025f,
                    Child = new BeatmapLogo
                    {
                        Anchor = Anchor.Centre,
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 400,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Children = new Drawable[]
                    {
                        playlist = new PlaylistOverlay
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                },
                new FillFlowContainer
                {
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
                                        buttons = new Container
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
                                                        new MusicOverlayButton(FontAwesome.Solid.ArrowLeft)
                                                        {
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
                                                        new MusicOverlayButton(FontAwesome.Solid.StepBackward)
                                                        {
                                                            Action = () => musicController.PreviousTrack(),
                                                            TooltipText = "上一首/从头开始",
                                                        },
                                                        new MusicOverlayButton(FontAwesome.Solid.Music)
                                                        {
                                                            Action = () => musicController.TogglePause(),
                                                            TooltipText = "切换暂停",
                                                        },
                                                        new MusicOverlayButton(FontAwesome.Solid.StepForward)
                                                        {
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
                                                        new MusicOverlayButton(FontAwesome.Solid.User)
                                                        {
                                                            Action = () => InvokeSolo(),
                                                            TooltipText = "在选歌界面中查看",
                                                        },
                                                        new MusicOverlayButton(FontAwesome.Solid.Atom)
                                                        {
                                                            Action = () => playlist.ToggleVisibility(),
                                                            TooltipText = "歌曲列表",
                                                        }
                                                    }
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
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = hoverCheckContainer = new HoverCheckContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                },
                idleTracker = new MouseIdleTracker(2000),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.ValueChanged += _ => updateComponentFromBeatmap(Beatmap.Value);
        }

        protected override void LoadComplete()
        {
            idleTracker.IsIdle.ValueChanged += _ => UpdateBarEffects();
            hoverCheckContainer.ScreenHovered.ValueChanged += _ => UpdateBarEffects();
            inputManager = GetContainingInputManager();
            bgBox.ScaleTo(1.1f);

            playlist.BeatmapSets.BindTo(musicController.BeatmapSets);

            ShowOverlays();

            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();
            var track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;

            if (track?.IsDummyDevice == false)
            {
                progressBarContainer.progressBar.EndTime = track.Length;
                progressBarContainer.progressBar.CurrentTime = track.CurrentTime;
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

            ((BackgroundScreenBeatmap)Background).BlurAmount.Value = BACKGROUND_BLUR;
        }

        public override bool OnExiting(IScreen next)
        {
            beatmapParallax.Hide();
            beatmapParallax.Expire();
            this.FadeOut(500, Easing.OutQuint);
            return base.OnExiting(next);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.Right:
                    musicController.NextTrack();
                    return true;

                case Key.Left:
                    musicController.PreviousTrack();
                    return true;

                case Key.Space:
                    musicController.TogglePause();
                    return true;

                case Key.Menu:
                    playlist.ToggleVisibility();
                    return true;

                case Key.Enter:
                    InvokeSolo();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private void InvokeSolo()
        {
            game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo);
        }

        private void UpdateBarEffects()
        {
            var mouseIdle = idleTracker.IsIdle.Value;

            if ( !hoverCheckContainer.ScreenHovered.Value )
            {
                ShowOverlays();
                return;
            }

            switch (mouseIdle)
            {
                case true:
                    TryHideOverlays();
                    break;

                case false:
                    ShowOverlays();
                    break;
            }
        }

        private void HideOverlays()
        {
            game?.Toolbar.Hide();
            bgBox.FadeTo(0.3f, DURATION, Easing.OutQuint);
            buttons.MoveToY(20, DURATION, Easing.OutQuint);
            bottomBar.ResizeHeightTo(0, DURATION, Easing.OutQuint)
                     .FadeTo(0.01f, DURATION, Easing.OutQuint);
            AllowBack = false;
            AllowCursor = false;
            ScheduleDone = true;
        }

        
        /// <summary>
        /// 因为未知原因, <see cref="TryHideOverlays"/>调用的<see cref="HideOverlays"/>无法被<see cref="ShowOverlays"/>中断
        /// 因此将相关功能独立出来作为单独的函数用来调用
        /// </summary>
        private void RunHideOverlays()
        {
            if ( !idleTracker.IsIdle.Value || !hoverCheckContainer.ScreenHovered.Value || bottomBar.panel_IsHovered.Value )
                return;

            HideOverlays();
        }

        private void ShowOverlays()
        {
            scheduledHideBars?.Cancel();

            game?.Toolbar.Show();
            bgBox.FadeTo(0.6f, DURATION, Easing.OutQuint);
            buttons.MoveToY(0, DURATION, Easing.OutQuint);
            bottomBar.ResizeHeightTo(BOTTOMPANEL_SIZE.Y, DURATION, Easing.OutQuint)
                     .FadeIn(DURATION, Easing.OutQuint);
            AllowCursor = true;
            AllowBack = true;
            ScheduleDone = false;
        }

        private void TryHideOverlays()
        {
            try
            {
                if ( !canReallyHide || ScheduleDone || bottomBar.panel_IsHovered.Value)
                    return;

                scheduledHideBars = Scheduler.AddDelayed(() =>
                {
                    RunHideOverlays();
                }, 1000);
            }
            finally
            {
                Schedule(TryHideOverlays);
            }
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            if (Background is BackgroundScreenBeatmap backgroundBeatmap)
            {
                backgroundBeatmap.Beatmap = beatmap;
                backgroundBeatmap.BlurAmount.Value = BACKGROUND_BLUR;
            }
        }

        private class HoverCheckContainer : Container
        {
            public readonly Bindable<bool> ScreenHovered = new Bindable<bool>();

            protected override bool OnHover(Framework.Input.Events.HoverEvent e)
            {
                this.ScreenHovered.Value = true;
                return base.OnHover(e);
            }

            protected override void OnHoverLost(Framework.Input.Events.HoverLostEvent e)
            {
                this.ScreenHovered.Value = false;
                base.OnHoverLost(e);
            }
        }
    }
}
