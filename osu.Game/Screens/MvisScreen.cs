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

namespace osu.Game.Screens
{
    /// <summary>
    /// 缝合怪 + 奥利给山警告
    /// </summary>
    public class MvisScreen : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);
        protected const float BACKGROUND_BLUR = 20;

        private const float DURATION = 750;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);
        private BottomBar bottomBar;
        private bool ScheduleDone = false;

        Container buttons;
        HoverCheckContainer hoverCheckContainer;
        ParallaxContainer beatmapParallax;
        private Box bgBox;

        private bool AllowCursor = false;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;
        public override bool AllowExternalScreenChange => true;

        private ScheduledDelegate scheduledHideBars;
        private InputManager inputManager { get; set; }
        private bool canReallyHide =>
            // don't hide if the user is hovering one of the panes, unless they are idle.
            (IsHovered || idleTracker.IsIdle.Value)
            // don't hide if a focused overlay is visible, like settings.
            && inputManager?.FocusedDrawable == null
            // don't hide if the user is hovering toolbar.
            && !game.Toolbar.toolbarIsHovered.Value;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        private MouseIdleTracker idleTracker;

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

            ShowHostOverlay();

            base.LoadComplete();
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
            }

            return base.OnKeyDown(e);
        }

        private void UpdateBarEffects()
        {
            var mouseIdle = idleTracker.IsIdle.Value;

            if ( !hoverCheckContainer.ScreenHovered.Value )
            {
                ShowHostOverlay();
                return;
            }

            switch (mouseIdle)
            {
                case true:
                    TryHideHostOverlay();
                    break;

                case false:
                    ShowHostOverlay();
                    break;
            }
        }

        private void HideHostOverlay()
        {
            if ( !idleTracker.IsIdle.Value  )
                return;

            game?.Toolbar.Hide();
            bgBox.FadeTo(0.3f, DURATION, Easing.OutQuint);
            buttons.MoveToY(20, DURATION, Easing.OutQuint);
            bottomBar.ResizeHeightTo(BOTTOMPANEL_SIZE.Y - 45, DURATION, Easing.OutQuint)
                     .FadeTo(0.01f, DURATION, Easing.OutQuint);
            AllowBack = false;
            AllowCursor = false;
            ScheduleDone = true;
        }

        private void ShowHostOverlay()
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

        private void TryHideHostOverlay()
        {
            try
            {
                if ( !canReallyHide || ScheduleDone || bottomBar.panel_IsHovered.Value)
                    return;

                scheduledHideBars = Scheduler.AddDelayed(() =>
                {
                    HideHostOverlay();
                }, 1000);

            }
            finally
            {
                    Schedule(TryHideHostOverlay);
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
