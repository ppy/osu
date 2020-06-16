// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu.Scoring;

namespace osu.Game.Rulesets.Osu.Statistics
{
    public class TimingDistributionGraph : CompositeDrawable
    {
        /// <summary>
        /// The number of data points shown on the axis below the graph.
        /// </summary>
        private const float axis_points = 5;

        /// <summary>
        /// An amount to adjust the value of the axis points by, effectively insetting the axis in the graph.
        /// Without an inset, the final data point will be placed halfway outside the graph.
        /// </summary>
        private const float axis_value_inset = 0.2f;

        private readonly TimingDistribution distribution;

        public TimingDistributionGraph(TimingDistribution distribution)
        {
            this.distribution = distribution;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (distribution?.Bins == null || distribution.Bins.Length == 0)
                return;

            int maxCount = distribution.Bins.Max();

            var bars = new Drawable[distribution.Bins.Length];
            for (int i = 0; i < bars.Length; i++)
                bars[i] = new Bar { Height = (float)distribution.Bins[i] / maxCount };

            Container axisFlow;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[] { bars }
                        }
                    },
                    new Drawable[]
                    {
                        axisFlow = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        }
                    },
                },
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                }
            };

            // We know the total number of bins on each side of the centre ((n - 1) / 2), and the size of each bin.
            // So our axis will contain one centre element + 5 points on each side, each with a value depending on the number of bins * bin size.
            int sideBins = (distribution.Bins.Length - 1) / 2;
            double maxValue = sideBins * distribution.BinSize;
            double axisValueStep = maxValue / axis_points * (1 - axis_value_inset);

            axisFlow.Add(new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = "0",
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold)
            });

            for (int i = 1; i <= axis_points; i++)
            {
                double axisValue = i * axisValueStep;
                float position = (float)(axisValue / maxValue);
                float alpha = 1f - position * 0.8f;

                axisFlow.Add(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    X = -position / 2,
                    Alpha = alpha,
                    Text = axisValue.ToString("-0"),
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold)
                });

                axisFlow.Add(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    X = position / 2,
                    Alpha = alpha,
                    Text = axisValue.ToString("+0"),
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold)
                });
            }
        }

        private class Bar : CompositeDrawable
        {
            public Bar()
            {
                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;

                RelativeSizeAxes = Axes.Both;

                Padding = new MarginPadding { Horizontal = 1 };

                InternalChild = new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#66FFCC")
                };
            }
        }
    }
}
