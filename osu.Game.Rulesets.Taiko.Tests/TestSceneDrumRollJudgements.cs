// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneDrumRollJudgements : TestSceneTaikoPlayer
    {
        [Test]
        public void TestStrongDrumRollFullyJudgedOnKilled()
        {
            AddUntilStep("gameplay finished", () => Player.ScoreProcessor.HasCompleted.Value);
            AddAssert("all judgements are misses", () => Player.Results.All(r => r.Type == r.Judgement.MinResult));
        }

        protected override bool Autoplay => false;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<TaikoHitObject>
        {
            BeatmapInfo = { Ruleset = new TaikoRuleset().RulesetInfo },
            HitObjects =
            {
                new DrumRoll
                {
                    StartTime = 1000,
                    Duration = 1000,
                    IsStrong = true
                }
            }
        };
    }
}
