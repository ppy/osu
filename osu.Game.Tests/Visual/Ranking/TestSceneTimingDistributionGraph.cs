// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Statistics;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneTimingDistributionGraph : OsuTestScene
    {
        public TestSceneTimingDistributionGraph()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#333")
                },
                new TimingDistributionGraph(CreateNormalDistribution())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(400, 130)
                }
            };
        }

        public static TimingDistribution CreateNormalDistribution()
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
}
