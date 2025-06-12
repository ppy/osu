// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        [TestCase(4)]
        [TestCase(7)]
        [TestCase(10)]
        public void TestColumnsAreReversed(int columnCount)
        {
            var original = createRawBeatmap(columnCount);
            var mirrored = createModdedBeatmap(columnCount);

            for (int i = 0; i < original.HitObjects.Count; i++)
            {
                var orig = original.HitObjects[i];
                var mirror = mirrored.HitObjects[i];

                int expectedColumn = columnCount - 1 - orig.Column;
                Assert.That(mirror.Column, Is.EqualTo(expectedColumn),
                    $"Object {i}: Expected column {expectedColumn}, but got {mirror.Column}");
            }
        }

        private static ManiaBeatmap createModdedBeatmap(int columnCount)
        {
            var beatmap = createRawBeatmap(columnCount);
            var mod = new ManiaModMirror();

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
