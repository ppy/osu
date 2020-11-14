// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using JetBrains.Annotations;
using static osu.Game.Users.User;
using System;
using System.Linq;
using osu.Game.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class ProfileLineChart : CompositeDrawable
    {
        private UserHistoryCount[] values;

        [CanBeNull]
        public UserHistoryCount[] Values
        {
            get => values;
            set
            {
                values = value;
                graph.Values = values;

                createRowTicks();
                createColumnTicks();
            }
        }

        private readonly UserHistoryGraph graph;
        private readonly Container<TickText> rowTicksContainer;
        private readonly Container<TickText> columnTicksContainer;
        private readonly Container<TickLine> rowLinesContainer;
        private readonly Container<TickLine> columnLinesContainer;

        public ProfileLineChart()
        {
            RelativeSizeAxes = Axes.X;
            Height = 250;
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension()
                },
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        rowTicksContainer = new Container<TickText>
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        rowLinesContainer = new Container<TickLine>
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        columnLinesContainer = new Container<TickLine>
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        }
                                    }
                                },
                                graph = new UserHistoryGraph
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        }
                    },
                    new Drawable[]
                    {
                        Empty(),
                        columnTicksContainer = new Container<TickText>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Top = 10 }
                        }
                    }
                }
            };
        }

        private void createRowTicks()
        {
            rowTicksContainer.Clear();
            rowLinesContainer.Clear();

            var min = values.Select(v => v.Count).Min();
            var max = values.Select(v => v.Count).Max();

            var niceRange = niceNumber(max - min, false);
            var niceTick = niceNumber(niceRange / (6 - 1), true);
            var axisStart = Math.Floor(min / niceTick) * niceTick;
            var axisEnd = Math.Ceiling(max / niceTick) * niceTick;

            var rollingRow = axisStart;

            while (rollingRow <= axisEnd)
            {
                var y = -Interpolation.ValueAt(rollingRow, 0, 1f, axisStart, axisEnd);

                rowTicksContainer.Add(new TickText
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.CentreRight,
                    RelativePositionAxes = Axes.Y,
                    Text = rollingRow.ToString("N0"),
                    Y = y
                });

                rowLinesContainer.Add(new TickLine
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.X,
                    RelativePositionAxes = Axes.Y,
                    Height = 1,
                    Y = y
                });

                rollingRow += niceTick;
            }
        }

        private void createColumnTicks()
        {
            columnTicksContainer.Clear();
            columnLinesContainer.Clear();

            var min = values.Select(v => v.Date).Min().Ticks;
            var max = values.Select(v => v.Date).Max().Ticks;

            var niceRange = niceNumber(max - min, false);
            var niceTick = niceNumber(niceRange / (Math.Min(values.Length, 15) - 1), true);
            var axisStart = Math.Floor(min / niceTick) * niceTick;
            var axisEnd = Math.Ceiling(max / niceTick) * niceTick;

            var rollingRow = axisStart;

            while (rollingRow <= axisEnd)
            {
                var x = Interpolation.ValueAt(rollingRow, 0, 1f, axisStart, axisEnd);

                columnTicksContainer.Add(new TickText
                {
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.X,
                    Text = new DateTime((long)rollingRow).ToString("MMM yyyy"),
                    Rotation = 45,
                    X = x
                });

                columnLinesContainer.Add(new TickLine
                {
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    Width = 1,
                    X = x
                });

                rollingRow += niceTick;
            }
        }

        private double niceNumber(double value, bool round)
        {
            var exponent = Math.Floor(Math.Log10(value));
            var fraction = value / Math.Pow(10, exponent);

            double niceFraction;

            if (round)
            {
                if (fraction < 1.5)
                    niceFraction = 1.0;
                else if (fraction < 3)
                    niceFraction = 2.0;
                else if (fraction < 7)
                    niceFraction = 5.0;
                else
                    niceFraction = 10.0;
            }
            else
            {
                if (fraction <= 1.0)
                    niceFraction = 1.0;
                else if (fraction <= 2.0)
                    niceFraction = 2.0;
                else if (fraction <= 5.0)
                    niceFraction = 5.0;
                else
                    niceFraction = 10.0;
            }

            return niceFraction * Math.Pow(10, exponent);
        }

        private class TickText : OsuSpriteText
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
                Font = OsuFont.GetFont(size: 12);
            }
        }

        private class TickLine : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Background6;
            }
        }
    }
}
