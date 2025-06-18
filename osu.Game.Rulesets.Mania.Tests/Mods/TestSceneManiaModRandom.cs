// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModRandom : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [TestCase(4)]
        [TestCase(7)]
        [TestCase(10)]
        public void TestNoOverlappingInSameColumn_Notes(int columnCount)
            => assertNoOverlapping(columnCount, ManiaModRandom.RandomizationType.Notes);

        [TestCase(4)]
        [TestCase(7)]
        [TestCase(10)]
        public void TestNoOverlappingInSameColumn_Columns(int columnCount)
            => assertNoOverlapping(columnCount, ManiaModRandom.RandomizationType.Columns);

        private void assertNoOverlapping(int columnCount, ManiaModRandom.RandomizationType mode)
        {
            var beatmap = createModdedBeatmap(columnCount, mode);

            for (int column = 0; column < columnCount; column++)
            {
                var columnObjects = beatmap.HitObjects
                                           .Where(o => o.Column == column)
                                           .OrderBy(o => o.StartTime)
                                           .ToList();

                for (int i = 1; i < columnObjects.Count; i++)
                {
                    var prev = columnObjects[i - 1];
                    var curr = columnObjects[i];

                    double prevEnd = (prev as HoldNote)?.EndTime ?? prev.StartTime;
                    Assert.That(prevEnd <= curr.StartTime,
                        $"[Mode: {mode}] Column {column}: Note at {curr.StartTime} overlaps with previous ending at {prevEnd}");
                }
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(10)]
        public void TestColumnRandomizationVaryingSeeds(int columnCount)
        {
            var original = createRawBeatmap(columnCount);
            var originalColumns = original.HitObjects.Cast<ManiaHitObject>().Select(h => h.Column).ToList();

            int unchangedCount = 0;

            for (int seed = 0; seed < 10000; seed++)
            {
                var beatmap = createRawBeatmap(columnCount);

                var mod = new ManiaModRandom
                {
                    Seed = { Value = seed },
                    Randomizer = { Value = ManiaModRandom.RandomizationType.Columns }
                };

                foreach (var obj in beatmap.HitObjects)
                    obj.ApplyDefaults(beatmap.ControlPointInfo, new BeatmapDifficulty());

                mod.ApplyToBeatmap(beatmap);

                var newColumns = beatmap.HitObjects.Cast<ManiaHitObject>().Select(h => h.Column).ToList();

                if (newColumns.SequenceEqual(originalColumns))
                    unchangedCount++;
            }

            Assert.That(unchangedCount < 10000, $"Expected at least one seed to produce different column assignments, but all 10000 seeds were unchanged.");
            TestContext.WriteLine($"{10000 - unchangedCount} out of 10000 seeds produced different column assignments.");
        }

        private static ManiaBeatmap createModdedBeatmap(int columnCount, ManiaModRandom.RandomizationType mode)
        {
            var beatmap = createRawBeatmap(columnCount);

            var mod = new ManiaModRandom
            {
                Randomizer = { Value = mode }
            };

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, new BeatmapDifficulty());

            mod.ApplyToBeatmap(beatmap);

            return beatmap;
        }

        private static ManiaBeatmap createRawBeatmap(int columnCount)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition(columnCount));
            beatmap.ControlPointInfo.Add(0.0, new TimingControlPoint { BeatLength = 500 });

            int time = 0;

            for (int i = 0; i < columnCount; i++)
            {
                beatmap.HitObjects.Add(new Note
                {
                    StartTime = time,
                    Column = i
                });
                time += 250;
            }

            for (int i = 0; i < columnCount; i++)
            {
                beatmap.HitObjects.Add(new HoldNote
                {
                    StartTime = time,
                    EndTime = time + 1000,
                    Column = i
                });
                time += 500;
            }

            return beatmap;
        }
    }
}
