// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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
        private Box innerCircleHighlight;

        private int currentLight;

        private Container scaleContainer;
        private Container lights;
        private Container lightsGlow;
        private OsuSpriteText text;

        private const int light_count = 6;

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(SIZE);

            const float ring_width = 18;
            const float light_padding = 3;

            InternalChild = scaleContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3
                    },
                    lights = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
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
                    new Circle
                    {
                        Name = "inner masking",
                        Size = new Vector2(SIZE - ring_width * 2 + light_padding * 2),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = colourProvider.Background3,
                    },
                    lightsGlow = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    innerCircle = new CircularContainer
                    {
                        Size = new Vector2(SIZE - ring_width * 2),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colourProvider.Background2,
                                RelativeSizeAxes = Axes.Both,
                            },
                            innerCircleHighlight = new Box
                            {
                                Colour = colourProvider.Colour3,
                                Blending = BlendingParameters.Additive,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                            },
                            text = new OsuSpriteText
                            {
                                Font = OsuFont.Torus.With(size: 20),
                                Colour = colourProvider.Background1,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Tap!"
                            },
                            hoverLayer = new Circle
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background1.Opacity(0.3f),
                                Blending = BlendingParameters.Additive,
                                Alpha = 0,
                            },
                        }
                    },
                }
            };

            for (int i = 0; i < light_count; i++)
            {
                var light = new Light
                {
                    Rotation = i * (360f / light_count)
                };

                lights.Add(light);
                lightsGlow.Add(light.Glow.CreateProxy());
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            hoverLayer.ReceivePositionalInputAt(screenSpacePos);

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
            const double in_duration = 100;

            text.FadeColour(colourProvider.Background4, in_duration, Easing.OutQuint);

            scaleContainer.ScaleTo(0.99f, in_duration, Easing.OutQuint);
            innerCircle.ScaleTo(0.96f, in_duration, Easing.OutQuint);

            innerCircleHighlight
                .FadeIn(50, Easing.OutQuint)
                .FlashColour(Color4.White, 1000, Easing.OutQuint);

            lights[currentLight % light_count].Hide();
            lights[(currentLight + light_count / 2) % light_count].Hide();

            currentLight++;

            lights[currentLight % light_count].Show();
            lights[(currentLight + light_count / 2) % light_count].Show();

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            const double out_duration = 800;

            text.FadeColour(colourProvider.Background1, out_duration, Easing.OutQuint);

            scaleContainer.ScaleTo(1, out_duration, Easing.OutQuint);
            innerCircle.ScaleTo(1, out_duration, Easing.OutQuint);
            innerCircleHighlight.FadeOut(out_duration, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        private class Light : CompositeDrawable
        {
            public Drawable Glow { get; private set; }

            private Container fillContent;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Size = new Vector2(0.98f); // Avoid bleed into masking edge.

                InternalChildren = new Drawable[]
                {
                    new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                        Current = { Value = 1f / light_count - 0.01f },
                        Colour = colourProvider.Background2,
                    },
                    fillContent = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = colourProvider.Colour1,
                        Children = new[]
                        {
                            new CircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Current = { Value = 1f / light_count - 0.01f },
                                Blending = BlendingParameters.Additive
                            },
                            Glow = new CircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Current = { Value = 1f / light_count - 0.01f },
                                Blending = BlendingParameters.Additive
                            }.WithEffect(new GlowEffect
                            {
                                Colour = colourProvider.Colour1.Opacity(0.4f),
                                BlurSigma = new Vector2(9f),
                                Strength = 10,
                                PadExtent = true
                            }),
                        }
                    },
                };
            }

            public override void Show()
            {
                fillContent
                    .FadeIn(50, Easing.OutQuint)
                    .FlashColour(Color4.White, 1000, Easing.OutQuint);
            }

            public override void Hide()
            {
                fillContent
                    .FadeOut(300, Easing.OutQuint);
            }
        }
    }
}
