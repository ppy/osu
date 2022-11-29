// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    public abstract partial class FirstRunSetupScreen : Screen
    {
        private const float offset = 100;

        protected FillFlowContainer Content { get; private set; }

        protected const float CONTENT_FONT_SIZE = 16;

        protected const float CONTENT_PADDING = 30;

        protected const float HEADER_FONT_SIZE = 24;

        [Resolved]
        protected OverlayColourProvider OverlayColourProvider { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float spacing = 20;

            InternalChildren = new Drawable[]
            {
                new OsuScrollContainer(Direction.Vertical)
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = false,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = CONTENT_PADDING },
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = this.GetLocalisableDescription(),
                                Font = OsuFont.TorusAlternate.With(size: HEADER_FONT_SIZE),
                                Colour = OverlayColourProvider.Light1,
                            },
                            Content = new FillFlowContainer
                            {
                                Y = HEADER_FONT_SIZE + spacing,
                                Spacing = new Vector2(spacing),
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                            }
                        },
                    },
                }
            };
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this
                .FadeInFromZero(100)
                .MoveToX(offset)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this
                .FadeInFromZero(100)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this
                .FadeOut(100)
                .MoveToX(offset, 500, Easing.OutQuint);

            return base.OnExiting(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this
                .FadeOut(100)
                .MoveToX(-offset, 500, Easing.OutQuint);

            base.OnSuspending(e);
        }
    }
}
