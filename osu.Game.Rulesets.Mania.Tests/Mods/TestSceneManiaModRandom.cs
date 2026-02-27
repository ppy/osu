// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
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

        [Test]
        public void TestNoOverlappingInSameColumn()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModRandom
                {
                    Randomizer = { Value = ManiaModRandom.RandomizationType.Notes }
                },
                Autoplay = true,
                CreateBeatmap = () => createBeatmap(7),
                PassCondition = () =>
                {
                    if (Player?.Beatmap.Value?.Beatmap is not ManiaBeatmap beatmap)
                        return false;

                    int columnCount = beatmap.TotalColumns;

                    for (int column = 0; column < columnCount; column++)
                    {
                        var columnObjects = beatmap.HitObjects.Where(obj => obj.Column == column).ToList();

                        columnObjects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

                        for (int i = 1; i < columnObjects.Count; i++)
                        {
                            var prev = columnObjects[i - 1];
                            var curr = columnObjects[i];
                            double prevEnd = (prev as HoldNote)?.EndTime ?? prev.StartTime;

                            if (prevEnd > curr.StartTime)
                                return false;
                        }
                    }

                    return true;
                }
            });
        }

        private static ManiaBeatmap createBeatmap(int columnCount)
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
