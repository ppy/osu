using System.Collections.Generic;
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

        private const int column_count = 4;

        [TestCase(ManiaModRandom.RandomizationType.Notes)]
        [TestCase(ManiaModRandom.RandomizationType.Columns)]
        [TestCase(ManiaModRandom.RandomizationType.Both)]
        public void TestNoOverlappingInSameColumn(ManiaModRandom.RandomizationType mode)
        {
            var beatmap = createModdedBeatmap(mode);

            for (int column = 0; column < column_count; column++)
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
                    Assert.That(prevEnd <= curr.StartTime, $"Mode: {mode}, Column {column}: Note at {curr.StartTime} overlaps with previous ending at {prevEnd}");
                }
            }
        }

        [TestCase(ManiaModRandom.RandomizationType.Notes)]
        [TestCase(ManiaModRandom.RandomizationType.Columns)]
        [TestCase(ManiaModRandom.RandomizationType.Both)]
        public void TestObjectCountRemainsSame(ManiaModRandom.RandomizationType mode)
        {
            var raw = createRawBeatmap();
            var modded = createModdedBeatmap(mode);

            Assert.That(modded.HitObjects.Count, Is.EqualTo(raw.HitObjects.Count), $"Mode: {mode}");
        }

        [TestCase(ManiaModRandom.RandomizationType.Notes)]
        [TestCase(ManiaModRandom.RandomizationType.Both)]
        public void TestAllColumnsUsedInRandomization(ManiaModRandom.RandomizationType mode)
        {
            var beatmap = createModdedBeatmap(mode);

            var usedColumns = beatmap.HitObjects.Select(o => o.Column).Distinct().ToList();
            usedColumns.Sort();
            Assert.That(usedColumns.Count, Is.EqualTo(column_count), $"Mode: {mode}. Expected all {column_count} columns to be used, but got: {string.Join(",", usedColumns)}");
        }

        [Test]
        public void TestColumnShuffleMappingIsValid()
        {
            var raw = createRawBeatmap();
            var modded = createModdedBeatmap(ManiaModRandom.RandomizationType.Columns);

            var rawCounts = countColumns(raw.HitObjects);
            var moddedCounts = countColumns(modded.HitObjects);

            var sortedRawCounts = rawCounts.OrderByDescending(x => x.Value).Select(x => x.Value);
            var sortedModdedCounts = moddedCounts.OrderByDescending(x => x.Value).Select(x => x.Value);

            Assert.That(sortedRawCounts, Is.EqualTo(sortedModdedCounts), "Expected column count distribution to match after shuffle.");
        }

        private Dictionary<int, int> countColumns(IEnumerable<ManiaHitObject> hitObjects)
        {
            var counts = new Dictionary<int, int>();

            foreach (var obj in hitObjects)
            {
                counts.TryAdd(obj.Column, 0);
                counts[obj.Column]++;
            }

            return counts;
        }

        private static ManiaBeatmap createModdedBeatmap(ManiaModRandom.RandomizationType mode)
        {
            var beatmap = createRawBeatmap();

            var mod = new ManiaModRandom
            {
                Randomizer = { Value = mode }
            };

            foreach (var obj in beatmap.HitObjects)
                obj.ApplyDefaults(beatmap.ControlPointInfo, new BeatmapDifficulty());

            mod.ApplyToBeatmap(beatmap);

            return beatmap;
        }

        private static ManiaBeatmap createRawBeatmap()
        {
            var beatmap = new ManiaBeatmap(new StageDefinition(column_count));
            beatmap.ControlPointInfo.Add(0.0, new TimingControlPoint { BeatLength = 500 });

            int time = 0;

            for (int i = 0; i < 16; i++)
            {
                beatmap.HitObjects.Add(new Note
                {
                    StartTime = time,
                    Column = i % column_count
                });
                time += 250;
            }

            for (int i = 0; i < 8; i++)
            {
                beatmap.HitObjects.Add(new HoldNote
                {
                    StartTime = time,
                    EndTime = time + 1000,
                    Column = i % column_count
                });
                time += 500;
            }

            return beatmap;
        }
    }
}
