// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using System;
using System.Linq;
using osu.Game.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public partial class ProfileLineChart : CompositeDrawable
    {
        private APIUserHistoryCount[] values = Array.Empty<APIUserHistoryCount>();

        public APIUserHistoryCount[] Values
        {
            get => values;
            set
            {
                if (value.Length == 0)
                    throw new ArgumentException("At least one value expected!", nameof(value));

                graph.Values = values = value;

                createRowTicks();
                createColumnTicks();
            }
        }

        private readonly UserHistoryGraph graph;
        private readonly Container<TickText> rowTicksContainer;
        private readonly Container<TickText> columnTicksContainer;
        private readonly Container<TickLine> rowLinesContainer;
        private readonly Container<TickLine> columnLinesContainer;

        public ProfileLineChart(LocalisableString graphCounterName)
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
                                graph = new UserHistoryGraph(graphCounterName)
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        }
                    },
                    new[]
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

            long min = values.Select(v => v.Count).Min();
            long max = values.Select(v => v.Count).Max();

            long tickInterval = getTickInterval(max - min, 6);

            for (long currentTick = 0; currentTick <= max; currentTick += tickInterval)
            {
                if (currentTick < min)
                    continue;

                float y;

                // special-case the min == max case to match LineGraph.
                // lerp isn't really well-defined over a zero interval anyway.
                if (min == max)
                    y = currentTick > 1 ? 1 : 0;
                else
                    y = Interpolation.ValueAt(currentTick, 0, 1f, min, max);

                // y axis is inverted in graph-like coordinates.
                addRowTick(-y, currentTick);
            }
        }

        private void createColumnTicks()
        {
            columnTicksContainer.Clear();
            columnLinesContainer.Clear();

            int totalMonths = values.Length;

            int monthsPerTick = 1;

            if (totalMonths > 80)
                monthsPerTick = 12;
            else if (totalMonths >= 45)
                monthsPerTick = 3;
            else if (totalMonths > 20)
                monthsPerTick = 2;

            for (int i = 0; i < totalMonths; i += monthsPerTick)
            {
                float x = (float)i / (totalMonths - 1);
                addColumnTick(x, values[i].Date);
            }
        }

        private void addRowTick(float y, double value)
        {
            rowTicksContainer.Add(new TickText
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.CentreRight,
                RelativePositionAxes = Axes.Y,
                Margin = new MarginPadding { Right = 3 },
                Text = value.ToLocalisableString("N0"),
                Font = OsuFont.GetFont(size: 12),
                Y = y
            });

            rowLinesContainer.Add(new TickLine
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.Y,
                Height = 0.1f,
                EdgeSmoothness = Vector2.One,
                Y = y
            });
        }

        private void addColumnTick(float x, DateTime value)
        {
            columnTicksContainer.Add(new TickText
            {
                Origin = Anchor.CentreLeft,
                RelativePositionAxes = Axes.X,
                Text = value.ToLocalisableString("MMM yyyy"),
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                Rotation = 45,
                X = x
            });

            columnLinesContainer.Add(new TickLine
            {
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Y,
                RelativePositionAxes = Axes.X,
                Width = 0.1f,
                EdgeSmoothness = Vector2.One,
                X = x
            });
        }

        private long getTickInterval(long range, int maxTicksCount)
        {
            // this interval is what would be achieved if the interval was divided perfectly evenly into maxTicksCount ticks.
            // can contain ugly fractional parts.
            float exactTickInterval = (float)range / (maxTicksCount - 1);

            // the ideal ticks start with a 1, 2 or 5, and are multipliers of powers of 10.
            // first off, use log10 to calculate the number of digits in the "exact" interval.
            double numberOfDigits = Math.Floor(Math.Log10(exactTickInterval));
            double tickBase = Math.Pow(10, numberOfDigits);

            // then see how the exact tick relates to the power of 10.
            double exactTickMultiplier = exactTickInterval / tickBase;

            double tickMultiplier;

            // round up the fraction to start with a 1, 2 or 5. closest match wins.
            if (exactTickMultiplier < 1.5)
                tickMultiplier = 1.0;
            else if (exactTickMultiplier < 3)
                tickMultiplier = 2.0;
            else if (exactTickMultiplier < 7)
                tickMultiplier = 5.0;
            else
                tickMultiplier = 10.0;

            return Math.Max((long)(tickMultiplier * tickBase), 1);
        }

        private partial class TickText : OsuSpriteText
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
            }
        }

        private partial class TickLine : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Background6;
            }
        }
    }
}
