// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneBananaShower : TestSceneCatchPlayer
    {
        [Test]
        public void TestBananaShower()
        {
            AddUntilStep("player is done", () => !Player.ValidForResume);
        }

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

            beatmap.HitObjects.Add(new BananaShower { StartTime = 200, Duration = 3000, NewCombo = true });

            return beatmap;
        }
    }
}
