// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Tests.Beatmaps
{
    public class BeatmapExtensionsTest
    {
        [Test]
        public void TestLengthCalculations()
        {
            var beatmap = new Beatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 5_000 },
                    new HitCircle { StartTime = 300_000 },
                    new Spinner { StartTime = 280_000, Duration = 40_000 }
                },
                Breaks =
                {
                    new BreakPeriod(50_000, 75_000),
                    new BreakPeriod(100_000, 150_000),
                }
            };

            Assert.That(beatmap.CalculatePlayableBounds(), Is.EqualTo((5_000, 320_000)));
            Assert.That(beatmap.CalculatePlayableLength(), Is.EqualTo(315_000)); // 320_000 - 5_000
            Assert.That(beatmap.CalculateDrainLength(), Is.EqualTo(240_000)); // 315_000 - (25_000 + 50_000) = 315_000 - 75_000
        }

        [Test]
        public void TestDrainLengthCannotGoNegative()
        {
            var beatmap = new Beatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 5_000 },
                    new HitCircle { StartTime = 300_000 },
                    new Spinner { StartTime = 280_000, Duration = 40_000 }
                },
                Breaks =
                {
                    new BreakPeriod(0, 350_000),
                }
            };

            Assert.That(beatmap.CalculatePlayableBounds(), Is.EqualTo((5_000, 320_000)));
            Assert.That(beatmap.CalculatePlayableLength(), Is.EqualTo(315_000)); // 320_000 - 5_000
            Assert.That(beatmap.CalculateDrainLength(), Is.EqualTo(0)); // break period encompasses entire beatmap
        }
    }
}
