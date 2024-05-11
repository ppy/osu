// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneCatchStacker : TestSceneCatchPlayer
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty { CircleSize = 6 },
                    Ruleset = ruleset
                }
            };

            for (int i = 0; i < 512; i++)
            {
                beatmap.HitObjects.Add(new Fruit
                {
                    X = (0.5f + i / 2048f * (i % 10 - 5)) * CatchPlayfield.WIDTH,
                    StartTime = i * 100,
                    NewCombo = i % 8 == 0
                });
            }

            return beatmap;
        }
    }
}
