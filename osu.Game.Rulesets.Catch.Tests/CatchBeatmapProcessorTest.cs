// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;

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

        [Test]
        public void TestHardRockJuiceStreamTimeOffset()
        {
            var beatmap = new Beatmap<CatchHitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty { SliderMultiplier = 1, SliderTickRate = 1 }
                },
                HitObjects = new List<CatchHitObject>
                {
                    new JuiceStream
                    {
                        StartTime = 1000,
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(osuTK.Vector2.Zero),
                            new PathControlPoint(new osuTK.Vector2(0, 100))
                        }, 100),
                        X = 100
                    },
                    new Fruit
                    {
                        StartTime = 2200,
                        X = 110
                    }
                }
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 });

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.Difficulty);

            // Confirm calculated duration
            var juiceStream = (JuiceStream)beatmap.HitObjects[0];
            Assert.That(juiceStream.EndTime, Is.EqualTo(2000));

            var processor = new CatchBeatmapProcessor(beatmap)
            {
                HardRockOffsets = true
            };

            processor.ApplyPositionOffsets(beatmap);

            var fruit = beatmap.HitObjects[1];

            // If start time (1000) is used: diff = 2200 - 1000 = 1200 > 1000 -> Reset -> XOffset = 0.
            // If end time (2000) is used: diff = 2200 - 2000 = 200 < 1000 -> Offset applied -> XOffset != 0.
            // Position difference is 110 - 100 = 10.
            // 10 < 200 / 3 (66.6) is True.
            // ApplyOffset adds 10 to offsetPosition (starts at 110). New offsetPosition = 120.
            // XOffset = 120 - 110 = 10.

            // We expect the fix to result in offset being applied.
            Assert.That(fruit.XOffset, Is.Not.Zero, "Fruit should have HardRock offset applied if correct time is used.");
            Assert.That(fruit.XOffset, Is.EqualTo(10), "Fruit offset amount incorrect.");
        }
    }
}
