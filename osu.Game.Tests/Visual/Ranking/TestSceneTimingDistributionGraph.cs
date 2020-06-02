// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneTimingDistributionGraph : OsuTestScene
    {
        public TestSceneTimingDistributionGraph()
        {
            Add(new TimingDistributionGraph(createNormalDistribution())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 100)
            });
        }

        private TimingDistribution createNormalDistribution()
        {
            var distribution = new TimingDistribution(51, 5);

            // We create an approximately-normal distribution of 51 elements by using the 13th binomial row (14 initial elements) and subdividing the inner values twice.
            var row = new List<int> { 1 };
            for (int i = 0; i < 13; i++)
                row.Add(row[i] * (13 - i) / (i + 1));

            // Each subdivision yields 2n-1 total elements, so first subdivision will contain 27 elements, and the second will contain 53 elements.
            for (int div = 0; div < 2; div++)
            {
                var newRow = new List<int> { 1 };

                for (int i = 0; i < row.Count - 1; i++)
                {
                    newRow.Add((row[i] + row[i + 1]) / 2);
                    newRow.Add(row[i + 1]);
                }

                row = newRow;
            }

            // After the subdivisions take place, we're left with 53 values which we use the inner 51 of.
            for (int i = 1; i < row.Count - 1; i++)
                distribution.Bins[i - 1] = row[i];

            return distribution;
        }
    }

    public class TimingDistributionGraph : CompositeDrawable
    {
        private readonly TimingDistribution distribution;

        public TimingDistributionGraph(TimingDistribution distribution)
        {
            this.distribution = distribution;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            int maxCount = distribution.Bins.Max();

            var bars = new Drawable[distribution.Bins.Length];
            for (int i = 0; i < bars.Length; i++)
                bars[i] = new Bar { Height = (float)distribution.Bins[i] / maxCount };

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[] { bars }
            };
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
