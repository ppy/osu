// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Intro
{
    public partial class CoverReveal : CompositeDrawable
    {
        private readonly Container content;
        private readonly Box bottomLayer;
        private readonly Box middleLayer;
        private readonly Box topLayer;
        private readonly TrianglesV2 triangles;

        public CoverReveal(RankedPlayColourScheme colourScheme)
        {
            Padding = new MarginPadding { Horizontal = 100 };
            Masking = true;

            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = -50 },
                Children =
                [
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            triangles = new TrianglesV2
                            {
                                RelativeSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                ClampAxes = Axes.None,
                                Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.White.Opacity(0)),
                            },
                            bottomLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Shear = new Vector2(-0.1f, 0),
                                Colour = ColourInfo.GradientVertical(colourScheme.PrimaryDarkest, colourScheme.PrimaryDarkest.Opacity(0)),
                                Alpha = 0.5f,
                            },
                            middleLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Shear = new Vector2(0.1f, 0),
                                Colour = ColourInfo.GradientVertical(colourScheme.PrimaryDarker, colourScheme.Primary.Opacity(0.5f)),
                                Alpha = 0.75f,
                            },
                            topLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourScheme.Primary,
                                Shear = new Vector2(-0.1f, 0)
                            },
                        ]
                    },
                ]
            };
        }

        protected override void Update()
        {
            base.Update();

            Padding = new MarginPadding
            {
                Horizontal = -DrawHeight * 0.25f / 2
            };
        }

        public void Play()
        {
            content.MoveToX(50)
                   .MoveToX(-50, 4000);

            triangles.MoveToX(-0.75f, 3500, new CubicBezierEasingFunction(0.05, 1, 0, 1))
                     .FadeOut(2000);

            topLayer.ResizeWidthTo(0.0f, 2800, new CubicBezierEasingFunction(0.05, 1, 0, 1))
                    .TransformTo(nameof(Shear), new Vector2(0.1f, 0), 2800, Easing.OutPow10)
                    .Then()
                    .ResizeWidthTo(0, 500, Easing.InQuart);

            middleLayer
                .Delay(50)
                .ResizeWidthTo(0.15f, 2900, new CubicBezierEasingFunction(0.05, 1, 0, 1))
                .TransformTo(nameof(Shear), new Vector2(-0.15f, 0), 2900, Easing.OutPow10)
                .Then()
                .ResizeWidthTo(0, 500, Easing.InQuart)
                .TransformTo(nameof(Shear), new Vector2(-0.25f, 0), 500, Easing.InCubic);

            bottomLayer
                .Delay(100)
                .ResizeWidthTo(0.2f, 3000, new CubicBezierEasingFunction(0.05, 1, 0, 1))
                .TransformTo(nameof(Shear), new Vector2(0.3f, 0), 3000, Easing.OutPow10)
                .Then()
                .ResizeWidthTo(0, 500, Easing.InQuart)
                .TransformTo(nameof(Shear), new Vector2(0.5f, 0), 500, Easing.InCubic);
        }
    }
}
