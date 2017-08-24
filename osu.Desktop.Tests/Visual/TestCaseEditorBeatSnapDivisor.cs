using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics;
using OpenTK.Graphics;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseEditorBeatSnapDivisor : TestCase
    {
        public TestCaseEditorBeatSnapDivisor()
        {
            Add(new BeatSnapDivisorVisualiser
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        private class BeatSnapDivisorVisualiser : CompositeDrawable
        {
            private const float width = 94;
            private const float corner_radius = 5;
            private const float tick_size = 8;
            private const float triangle_size = 6;

            private static readonly int[] available_divisors = { 1, 2, 3, 4, 6, 8, 12, 16, 24, 32 };

            private readonly Container<Box> tickContainer;

            private readonly Box background;
            private readonly EquilateralTriangle selectionTriangle;

            private readonly BufferedContainer textGlow;
            private readonly SpriteText text;

            private int currentDivisorIndex = 3;

            public BeatSnapDivisorVisualiser()
            {
                AutoSizeAxes = Axes.Y;
                Width = width;

                Masking = true;
                CornerRadius = corner_radius;

                AddRangeInternal(new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    tickContainer = new Container<Box>
                    {
                        RelativeSizeAxes = Axes.X,
                        Padding = new MarginPadding { Left = corner_radius, Right = corner_radius },
                        Height = tick_size
                    },
                    new Container
                    {
                        Name = "Bottom",
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = tick_size * 2 },
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Left = corner_radius, Right = corner_radius },
                                Child = selectionTriangle = new EquilateralTriangle { Height = triangle_size }
                            }
                        }
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                float tickSeparation = 1f / (available_divisors.Length - 1);
                for (int i = 0; i < available_divisors.Length; i++)
                {
                    var newTick = new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        RelativePositionAxes = Axes.X,
                        Width = 1,
                        X = i * tickSeparation
                    };

                    if (available_divisors[i] >= 12)
                        newTick.Colour = colours.Red;
                    else if (available_divisors[i] >= 8)
                        newTick.Colour = colours.Yellow;
                    else if (available_divisors[i] >= 6)
                        newTick.Colour = colours.YellowLight;
                    else
                        newTick.Colour = colours.Gray5;

                    tickContainer.Add(newTick);
                }

                background.Colour = colours.Gray1;
            }
        }
    }
}
