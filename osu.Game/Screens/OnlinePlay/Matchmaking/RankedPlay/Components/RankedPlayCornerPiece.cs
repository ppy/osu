// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    [Cached]
    public partial class RankedPlayCornerPiece : VisibilityContainer
    {
        private readonly BufferedContainer background;
        private readonly Container bottomLayer;
        private readonly Container topLayer;

        protected override Container<Drawable> Content { get; }

        public RankedPlayCornerPiece(RankedPlayColourScheme colourScheme, Anchor anchor)
        {
            Size = new Vector2(345, 100);

            Anchor = Origin = anchor;

            InternalChildren =
            [
                background = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(
                        (anchor & Anchor.x0) != 0 ? 1 : -1,
                        (anchor & Anchor.y0) != 0 ? -1 : 1
                    ),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Rotation = -2,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Shear = new Vector2(-0.5f, 0),
                        Padding = new MarginPadding
                        {
                            Left = -60,
                            Bottom = -30,
                            Top = 20,
                            Right = 15,
                        },
                        Children =
                        [
                            bottomLayer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 20,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourScheme.PrimaryDarkest,
                                    Alpha = 0.2f,
                                    // This is a hack to work around alpha-blending issues when drawing on top of a transparent background without premultiplied alpha
                                    // This method requires that this Drawable is not drawn on top of anything else
                                    Blending = BlendingParameters.Mixture with
                                    {
                                        Destination = BlendingType.Zero,
                                        DestinationAlpha = BlendingType.Zero,
                                        Source = BlendingType.One,
                                        SourceAlpha = BlendingType.One,
                                    }
                                },
                            },
                            topLayer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(10),
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Child = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 15,
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourInfo.GradientHorizontal(colourScheme.Primary, colourScheme.PrimaryDarker.Opacity(0.35f)),
                                        Alpha = 0.75f
                                    },
                                },
                            }
                        ]
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = anchor,
                    Origin = anchor,
                    Margin = new MarginPadding(18),
                    Child = Content = new Container
                    {
                        Anchor = (anchor & Anchor.x0) != 0 ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = (anchor & Anchor.x0) != 0 ? Anchor.CentreLeft : Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            ];
        }

        public void OnHealthChanged(int health)
        {
            background.GrayscaleTo(health <= 0f ? 0.75f : 0, 300);
        }

        protected override void Update()
        {
            base.Update();

            Width = WidthFor(Parent!.ChildSize.X);
        }

        public static float WidthFor(float parentWidth) => float.Clamp(parentWidth * 0.25f, 250, 335);

        protected override void PopIn()
        {
            this.FadeIn(300);

            Content.Delay(150)
                   .MoveToX(0, 400, Easing.OutExpo)
                   .ScaleTo(1f, 400, Easing.OutExpo)
                   .FadeIn();

            background.MoveToY(0, 400, Easing.OutExpo);

            bottomLayer.RotateTo(0, 400, Easing.OutQuart);
            topLayer.RotateTo(0, 400, Easing.OutQuart);
        }

        protected override void PopOut()
        {
            this.FadeOut(300);

            background.MoveToY((Anchor & Anchor.y0) != 0 ? -60 : 60, 500, new CubicBezierEasingFunction(easeIn: 0.2, easeOut: 0.75));
            Content.MoveToX((Anchor & Anchor.x0) != 0 ? -200 : 200, 500, new CubicBezierEasingFunction(easeIn: 0.2, easeOut: 0.5))
                   .ScaleTo(0.5f, 400, Easing.OutCubic)
                   .FadeOut(200);

            bottomLayer.RotateTo(-25, 500, new CubicBezierEasingFunction(easeIn: 0.2, easeOut: 0.75));
            topLayer.RotateTo(25, 500, new CubicBezierEasingFunction(easeIn: 0.2, easeOut: 0.75));
        }
    }
}
