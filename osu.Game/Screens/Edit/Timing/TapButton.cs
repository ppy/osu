// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Timing
{
    internal class TapButton : CircularContainer
    {
        public const float SIZE = 100;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private Circle hoverLayer;
        private CircularContainer innerCircle;
        private Circle outerCircle;

        private Container scaleContainer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(SIZE);

            const float ring_width = 20;
            const float light_padding = 3;

            InternalChild = scaleContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    outerCircle = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3
                    },
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Name = "outer masking",
                        Masking = true,
                        BorderThickness = light_padding,
                        BorderColour = colourProvider.Background3,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = Color4.Black,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                        }
                    },
                    innerCircle = new CircularContainer
                    {
                        Size = new Vector2(SIZE - ring_width + light_padding),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        BorderThickness = light_padding,
                        BorderColour = colourProvider.Background3,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colourProvider.Background2,
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                            },
                        }
                    },
                    hoverLayer = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.01f),
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverLayer.FadeIn(500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLayer.FadeOut(500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            scaleContainer.ScaleTo(0.98f, 200, Easing.OutQuint);
            innerCircle.ScaleTo(0.95f, 200, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            scaleContainer.ScaleTo(1, 1200, Easing.OutQuint);
            innerCircle.ScaleTo(1, 1200, Easing.OutQuint);
            base.OnMouseUp(e);
        }
    }
}
