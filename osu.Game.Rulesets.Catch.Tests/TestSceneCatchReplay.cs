// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneCatchReplay : TestSceneCatchPlayer
    {
        protected override bool Autoplay => true;

        private const int object_count = 10;

        [Test]
        public void TestReplayCatcherPositionIsFramePerfect()
        {
            AddUntilStep("caught all fruits", () => Player.ScoreProcessor.Combo.Value == object_count);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo =
                {
                    Ruleset = ruleset,
                }
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint());

            for (int i = 0; i < object_count / 2; i++)
            {
                beatmap.HitObjects.Add(new Fruit
                {
                    StartTime = (i + 1) * 1000,
                    X = 0
                });
                beatmap.HitObjects.Add(new Fruit
                {
                    StartTime = (i + 1) * 1000 + 1,
                    X = CatchPlayfield.WIDTH
                });
            }

            return beatmap;
        }
    }
}
