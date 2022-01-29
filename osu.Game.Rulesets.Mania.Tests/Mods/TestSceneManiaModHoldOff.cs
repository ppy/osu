// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Visual;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public class TestSceneManiaModHoldOff : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestMapHasNoHoldNotes()
        {
            var testBeatmap = createModdedBeatmap();
            Assert.False(testBeatmap.HitObjects.OfType<HoldNote>().Any());
        }

        [Test]
        public void TestCorrectNoteValues()
        {
            var testBeatmap = createRawBeatmap();
            var noteValues = new List<double>(testBeatmap.HitObjects.OfType<HoldNote>().Count());

            foreach (HoldNote h in testBeatmap.HitObjects.OfType<HoldNote>())
            {
                noteValues.Add(ManiaModHoldOff.GetNoteDurationInBeatLength(h, testBeatmap));
            }

            noteValues.Sort();
            Assert.AreEqual(noteValues, new List<double> { 0.125, 0.250, 0.500, 1.000, 2.000 });
        }

        [TestCase(ManiaModHoldOff.BeatDivisors.Whole)]
        [TestCase(ManiaModHoldOff.BeatDivisors.Half)]
        [TestCase(ManiaModHoldOff.BeatDivisors.Quarter)]
        [TestCase(ManiaModHoldOff.BeatDivisors.Eighth)]
        public void TestCorrectObjectCount(ManiaModHoldOff.BeatDivisors minBeatSnap)
        {
            /*
                This test is to ensure that, given that end notes are enabled,
                the mod produces the expected number of objects when the mod is applied.
            */

            // Mod settings will be set to include the correct beat snap value
            var rawBeatmap = createRawBeatmap();
            var testBeatmap = createModdedBeatmap(minBeatSnap);

            // Calculate expected number of objects
            int expectedObjectCount = 0;
            double beatSnapValue = 1 / (Math.Pow(2, (int)minBeatSnap));

            foreach (ManiaHitObject h in rawBeatmap.HitObjects)
            {
                // Both notes and hold notes account for at least one object
                expectedObjectCount++;

                if (h.GetType() == typeof(HoldNote))
                {
                    double noteValue = ManiaModHoldOff.GetNoteDurationInBeatLength((HoldNote)h, rawBeatmap);

                    if (noteValue >= beatSnapValue)
                    {
                        // Should generate an end note if it's longer than the minimum note value
                        expectedObjectCount++;
                    }
                }
            }

            Assert.That(testBeatmap.HitObjects.Count == expectedObjectCount);
        }

        [Test]
        public void TestDifficultyIncrease()
        {
            // A lower minimum beat snap divisor should only make the map harder, never easier
            // (as more notes can be spawned)
            var beatmaps = new[]
            {
                createModdedBeatmap(),
                createModdedBeatmap(ManiaModHoldOff.BeatDivisors.Half),
                createModdedBeatmap(ManiaModHoldOff.BeatDivisors.Quarter),
                createModdedBeatmap(ManiaModHoldOff.BeatDivisors.Eighth),
                createModdedBeatmap(ManiaModHoldOff.BeatDivisors.Sixteenth)
            };

            double[] mapDifficulties = new double[beatmaps.Length];

            for (int i = 0; i < mapDifficulties.Length; i++)
            {
                var workingBeatmap = new TestWorkingBeatmap(beatmaps[i]);
                var difficultyCalculator = new ManiaDifficultyCalculator(new ManiaRuleset().RulesetInfo, workingBeatmap);
                mapDifficulties[i] = difficultyCalculator.Calculate().StarRating;

                if (i > 0)
                {
                    Assert.LessOrEqual(mapDifficulties[i - 1], mapDifficulties[i]);
                    Assert.LessOrEqual(beatmaps[i - 1].HitObjects.Count, beatmaps[i].HitObjects.Count);
                }
            }
        }

        private static ManiaBeatmap createModdedBeatmap(ManiaModHoldOff.BeatDivisors minBeatSnap = ManiaModHoldOff.BeatDivisors.Whole)
        {
            var beatmap = createRawBeatmap();
            var holdOffMod = new ManiaModHoldOff
            {
                MinBeatSnap = { Value = minBeatSnap }
            };
            Assert.AreEqual(holdOffMod.MinBeatSnap.Value, minBeatSnap);

            foreach (var hitObject in beatmap.HitObjects)
                hitObject.ApplyDefaults(beatmap.ControlPointInfo, new BeatmapDifficulty());

            holdOffMod.ApplyToBeatmap(beatmap);

            return beatmap;
        }

        private static ManiaBeatmap createRawBeatmap()
        {
            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 1 });
            beatmap.ControlPointInfo.Add(0.0, new TimingControlPoint { BeatLength = 1000 }); // Set BPM to 60

            // Add test hit objects
            beatmap.HitObjects.Add(new Note { StartTime = 4000 });
            beatmap.HitObjects.Add(new Note { StartTime = 4500 });
            beatmap.HitObjects.Add(new HoldNote { StartTime = 0, EndTime = 125 }); // 1/8 note
            beatmap.HitObjects.Add(new HoldNote { StartTime = 0, EndTime = 250 }); // 1/4 note
            beatmap.HitObjects.Add(new HoldNote { StartTime = 0, EndTime = 500 }); // 1/2 note
            beatmap.HitObjects.Add(new HoldNote { StartTime = 0, EndTime = 1000 }); // 1/1 note
            beatmap.HitObjects.Add(new HoldNote { StartTime = 0, EndTime = 2000 }); // 2/1 note

            return beatmap;
        }
    }
}
