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
    public partial class TestSceneManiaModMirror : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        private const int column_count = 4;

        [Test]
        public void TestColumnsAreReversed()
        {
            var original = createRawBeatmap();
            var mirrored = createModdedBeatmap();

            for (int i = 0; i < original.HitObjects.Count; i++)
            {
                var orig = original.HitObjects[i];
                var mirror = mirrored.HitObjects[i];

                int expectedColumn = column_count - 1 - orig.Column;
                Assert.That(mirror.Column, Is.EqualTo(expectedColumn),
                    $"Object {i}: Expected column {expectedColumn}, but got {mirror.Column}");
            }
        }

        [Test]
        public void TestAllOriginalPropertiesUnchangedExceptColumn()
        {
            var original = createRawBeatmap();
            var mirrored = createModdedBeatmap();

            for (int i = 0; i < original.HitObjects.Count; i++)
            {
                var orig = original.HitObjects[i];
                var mirror = mirrored.HitObjects[i];

                Assert.That(mirror.StartTime, Is.EqualTo(orig.StartTime), $"Object {i}: StartTime mismatch.");

                if (orig is HoldNote origHold && mirror is HoldNote mirrorHold)
                {
                    Assert.That(mirrorHold.EndTime, Is.EqualTo(origHold.EndTime), $"Object {i}: EndTime mismatch.");
                }
            }
        }

        [Test]
        public void TestColumnRangeIsValid()
        {
            var beatmap = createModdedBeatmap();

            var invalidColumns = beatmap.HitObjects.Where(o => o.Column < 0 || o.Column >= column_count).ToList();
            Assert.That(invalidColumns.Count, Is.EqualTo(0), $"Found objects in invalid columns: {string.Join(",", invalidColumns.Select(o => o.Column))}");
        }

        private static ManiaBeatmap createModdedBeatmap()
        {
            var beatmap = createRawBeatmap();

            var mod = new ManiaModMirror();

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

            for (int i = 0; i < column_count; i++)
            {
                beatmap.HitObjects.Add(new Note
                {
                    StartTime = time,
                    Column = i
                });
                time += 250;
            }

            for (int i = 0; i < column_count; i++)
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
