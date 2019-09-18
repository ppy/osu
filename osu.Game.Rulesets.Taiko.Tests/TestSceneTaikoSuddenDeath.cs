// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneTaikoSuddenDeath : PlayerTestScene
    {
        public TestSceneTaikoSuddenDeath()
            : base(new TaikoRuleset())
        {
        }

        protected override bool AllowFail => true;

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Mods.Value = Mods.Value.Concat(new[] { new TaikoModSuddenDeath() }).ToArray();
            return new ScoreAccessiblePlayer();
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) =>
            new TaikoBeatmap
            {
                HitObjects =
                {
                    new Swell { StartTime = 1500 },
                    new Hit { StartTime = 100000 },
                },
                BeatmapInfo =
                {
                    Ruleset = new TaikoRuleset().RulesetInfo
                }
            };

        [Test]
        public void TestSpinnerDoesNotFail()
        {
            bool judged = false;
            AddStep("Setup judgements", () =>
            {
                judged = false;
                ((ScoreAccessiblePlayer)Player).ScoreProcessor.NewJudgement += b => judged = true;
            });
            AddUntilStep("swell judged", () => judged);
            AddAssert("not failed", () => !Player.HasFailed);
        }

        private class ScoreAccessiblePlayer : TestPlayer
        {
            public ScoreAccessiblePlayer()
                : base(false, false)
            {
            }

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
        }
    }
}
