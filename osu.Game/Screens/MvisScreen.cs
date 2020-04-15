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
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Input;
using osu.Framework.Input;

namespace osu.Game.Screens
{
    /// <summary>
    /// 缝 合 怪
    /// </summary>
    public class MvisScreen : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);
        protected const float BACKGROUND_BLUR = 20;

        private const float DURATION = 750;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);
        private BottomBar bottomBar;

        Container buttons;
        private Box bgBox;

        protected OsuScreenStack ScreenStack;
        //private bool AllowCursor = false;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        //public override bool CursorVisible => AllowCursor;

        private InputManager inputManager;
        private IdleTracker idleTracker;
        private bool canReallyHide =>
            // don't push if the user is hovering one of the panes, unless they are idle.
            (IsHovered || idleTracker.IsIdle.Value)
            // don't push if the user is dragging a slider or otherwise.
            && inputManager?.DraggedDrawable == null
            // don't push if a focused overlay is visible, like settings.
            && inputManager?.FocusedDrawable == null;


        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }


        [Resolved]
        private MusicController musicController { get; set; }
        [Resolved]
        private OsuColour colours { get; set; }

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
                new ParallaxContainer
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
                                                    TooltipText = "上一首",
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
                                                new MusicOverlayButton(FontAwesome.Solid.TheaterMasks)
                                                {
                                                    TooltipText = "工具栏、底栏常驻(未实现)",
                                                },
                                            }
                                        },
                                    }
                                },
                            }
                        },
                    }
                },
                idleTracker = new IdleTracker(1000),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            game?.Toolbar.Hide();
            Beatmap.ValueChanged += _ => updateComponentFromBeatmap(Beatmap.Value);
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
            bottomBar.panel_IsHovered.ValueChanged += _ => UpdateBarEffects();

            HideBottomBar(true);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            ((BackgroundScreenBeatmap)Background).BlurAmount.Value = BACKGROUND_BLUR;
        }

        private void UpdateBarEffects()
        {
            switch (bottomBar.panel_IsHovered.Value)
            {
                case true:
                    ShowBottomBar();
                    break;

                case false:
                    this.Delay(1000).Schedule( () => HideBottomBar(false) );
                    break;
            }
        }

        private void HideBottomBar(bool IgnoreCanReallyHide = false)
        {
            try
            {
                if ( IgnoreCanReallyHide == false && (bottomBar.panel_IsHovered.Value || !canReallyHide) )
                    return;
            }
            finally
            {
                game?.Toolbar.Hide();
                AllowBack = false;
                bgBox.FadeTo(0.3f, 500, Easing.OutQuint);
                //AllowCursor = false;
                bottomBar.ResizeHeightTo(BOTTOMPANEL_SIZE.Y - 45, DURATION, Easing.OutQuint)
                         .FadeTo(0.01f, DURATION, Easing.OutQuint);
            }
        }

        private void ShowBottomBar()
        {
            game?.Toolbar.Show();
            //AllowCursor = true;
            bgBox.FadeTo(0.6f, 500, Easing.OutQuint);
            AllowBack = true;
            bottomBar.ResizeHeightTo(BOTTOMPANEL_SIZE.Y, DURATION, Easing.OutQuint)
                     .FadeIn(DURATION, Easing.OutQuint);
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            if (Background is BackgroundScreenBeatmap backgroundModeBeatmap)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurAmount.Value = BACKGROUND_BLUR;
                backgroundModeBeatmap.FadeColour(Color4.White, 250);
            }
        }

        private class HoverableProgressBar : ProgressBar
        {
            protected override bool OnHover(HoverEvent e)
            {
                this.ResizeHeightTo(10, 500, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.ResizeHeightTo(10 / 2, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
