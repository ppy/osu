// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchBeatmapProcessorTest
    {
        [Test]
        public void TestHardRockOffsetDoublePrecision()
        {
            // Setup a beatmap with two fruits that will trigger the logic difference.
            // We need a time difference that has a fractional part.
            // And we need HardRock enabled.

            // lastPosition = 100, lastStartTime = 1000.
            // offsetPosition = 133.2, startTime = 1100.5.

            // positionDiff = 33.2.
            // timeDiff (int) = 100.
            // timeDiff (double) = 100.5.

            // positionDiff < timeDiff / 3
            // 33.2 < 33 (False)
            // 33.2 < 33.5 (True)

            var beatmap = new Beatmap<CatchHitObject>
            {
                HitObjects = new List<CatchHitObject>
                {
                    new Fruit { StartTime = 1000, X = 100 },
                    new Fruit { StartTime = 1000 + 100.5, X = 100 + 33.2f }
                }
            };

            var processor = new CatchBeatmapProcessor(beatmap)
            {
                HardRockOffsets = true
            };

            processor.ApplyPositionOffsets(beatmap);

            var secondObj = beatmap.HitObjects[1];

            // If bug is present (int truncation), condition is false, XOffset is 0.
            // If fixed (double), condition is true, XOffset is 33.2 (approx).

            Assert.That(secondObj.XOffset, Is.Not.EqualTo(0).Within(0.001));
            Assert.That(secondObj.XOffset, Is.EqualTo(33.2f).Within(0.001));
        }
    }
}
