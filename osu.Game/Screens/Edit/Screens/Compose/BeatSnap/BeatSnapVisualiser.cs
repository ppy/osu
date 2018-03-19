// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.BeatSnap
{
    public class BeatSnapVisualiser : CompositeDrawable
    {
        private static readonly int[] available_divisors = { 1, 2, 3, 4, 6, 8, 12, 16 };

        public readonly Bindable<int> Divisor = new Bindable<int>(1);
        private int currentDivisorIndex;

        private TickContainer tickContainer;
        private DivisorText text;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Name = "Background",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            tickContainer = new TickContainer(1, 2, 3, 4, 6, 8, 12, 16)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 5 }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Gray4
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = 5 },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.fa_chevron_left,
                                                        Action = selectPrevious
                                                    },
                                                    text = new DivisorText(),
                                                    new DivisorButton
                                                    {
                                                        Icon = FontAwesome.fa_chevron_right,
                                                        Action = selectNext
                                                    }
                                                },
                                                new Drawable[]
                                                {
                                                    null,
                                                    new TextFlowContainer(s => s.TextSize = 10)
                                                    {
                                                        Text = "beat snap divisor",
                                                        RelativeSizeAxes = Axes.X,
                                                        TextAnchor = Anchor.TopCentre
                                                    },
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.Absolute, 20),
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 20)
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 35),
                    }
                }
            };

            tickContainer.Divisor.BindTo(Divisor);
            text.Divisor.BindTo(Divisor);
        }

        private void selectPrevious()
        {
            if (currentDivisorIndex == 0)
                return;
            Divisor.Value = available_divisors[--currentDivisorIndex];
        }

        private void selectNext()
        {
            if (currentDivisorIndex == available_divisors.Length - 1)
                return;
            Divisor.Value = available_divisors[++currentDivisorIndex];
        }

        private class DivisorText : SpriteText
        {
            public readonly Bindable<int> Divisor = new Bindable<int>();

            public DivisorText()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Divisor.ValueChanged += v => updateText();
                updateText();
            }

            private void updateText() => Text = $"1/{Divisor.Value}";
        }

        private class DivisorButton : IconButton
        {
            public DivisorButton()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                // Small offset to look a bit better centered along with the divisor text
                Y = 1;

                ButtonSize = new Vector2(20);
                IconScale = new Vector2(0.6f);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconColour = Color4.Black;
                HoverColour = colours.Gray7;
                FlashColour = colours.Gray9;
            }
        }

        private class TickContainer : CompositeDrawable
        {
            public readonly BindableInt Divisor = new BindableInt();

            public new MarginPadding Padding
            {
                set => base.Padding = value;
            }

            private EquilateralTriangle marker;

            private readonly int[] availableDivisors;
            private readonly float tickSpacing;

            public TickContainer(params int[] divisors)
            {
                availableDivisors = divisors;
                tickSpacing = 1f / (availableDivisors.Length + 1);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChild = marker = new EquilateralTriangle
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomCentre,
                    RelativePositionAxes = Axes.X,
                    Height = 7,
                    EdgeSmoothness = new Vector2(1),
                    Colour = colours.Gray4,
                };

                for (int i = 0; i < availableDivisors.Length; i++)
                {
                    AddInternal(new Tick(availableDivisors[i])
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        X = getTickPosition(i)
                    });
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Divisor.ValueChanged += v => updatePosition();
                updatePosition();
            }

            private void updatePosition() => marker.MoveToX(getTickPosition(Array.IndexOf(availableDivisors, Divisor.Value)), 100, Easing.OutQuint);

            private float getTickPosition(int index) => (index + 1) * tickSpacing;

            private class Tick : Box
            {
                private readonly int divisor;

                public Tick(int divisor)
                {
                    this.divisor = divisor;

                    Size = new Vector2(2, 10);
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    if (divisor >= 16)
                        Colour = colours.Red;
                    else if (divisor >= 8)
                        Colour = colours.Yellow;
                    else
                        Colour = colours.Gray4;
                }
            }
        }
    }
}
