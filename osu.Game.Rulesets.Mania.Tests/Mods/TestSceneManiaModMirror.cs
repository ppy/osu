// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModMirror : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestMirrorFlipsColumns()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModMirror(),
                Autoplay = true,
                CreateBeatmap = () => createBeatmap(7),
                PassCondition = () =>
                {
                    if (Player?.Beatmap.Value?.Beatmap is not ManiaBeatmap mirrored)
                        return false;

                    if (mirrored.HitObjects.Count == 0)
                        return false;

                    var original = createBeatmap(mirrored.TotalColumns);
                    int maxCol = mirrored.TotalColumns - 1;

                    return !original.HitObjects.Select(t => maxCol - t.Column).Where((expected, i) => mirrored.HitObjects[i].Column != expected).Any();
                }
            });
        }

        private ManiaBeatmap createBeatmap(int columns)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition(columns));
            double t = 0;

            for (int i = 0; i < columns; i++)
            {
                beatmap.HitObjects.Add(new Note
                {
                    StartTime = t += 500,
                    Column = i
                });
            }

            return beatmap;
        }
    }
}
