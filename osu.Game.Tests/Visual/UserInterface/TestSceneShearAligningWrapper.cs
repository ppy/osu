// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearAligningWrapper : OsuTestScene
    {
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private ShearedBox first = null!;
        private ShearedBox second = null!;
        private ShearedBox third = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 200f,
                AutoSizeAxes = Axes.Y,
                Shear = OsuGame.SHEAR,
                CornerRadius = 10f,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background6,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, 10f),
                        Children = new Drawable[]
                        {
                            new ShearAligningWrapper(first = new ShearedBox("Text 1", OsuColour.Gray(0.4f))
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 30,
                            }),
                            new ShearAligningWrapper(second = new ShearedBox("Text 2", OsuColour.Gray(0.3f))
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 30,
                            }),
                            new ShearAligningWrapper(third = new ShearedBox("Text 3", OsuColour.Gray(0.2f))
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 30,
                            }),
                        }
                    }
                },
            };
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddSliderStep("box 1 height", 0, 100, 30, h =>
            {
                if (first.IsNotNull())
                    first.Height = h;
            });
            AddSliderStep("box 2 height", 0, 100, 30, h =>
            {
                if (second.IsNotNull())
                    second.Height = h;
            });
            AddSliderStep("box 3 height", 0, 100, 30, h =>
            {
                if (third.IsNotNull())
                    third.Height = h;
            });
        }

        public partial class ShearedBox : Container
        {
            private readonly string text;
            private readonly Color4 boxColour;

            public ShearedBox(string text, Color4 boxColour)
            {
                this.text = text;
                this.boxColour = boxColour;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                CornerRadius = 10;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = boxColour,
                    },
                    new OsuSpriteText
                    {
                        Text = text,
                        Colour = Color4.White,
                        Shear = -OsuGame.SHEAR,
                        Font = OsuFont.Torus.With(size: 24),
                        Margin = new MarginPadding { Left = 50 },
                    }
                };
            }
        }
    }
}
