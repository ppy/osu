// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModDifficultyAdjust : ModTestScene
    {
        public TestSceneOsuModDifficultyAdjust()
            : base(new OsuRuleset())
        {
        }

        [Test]
        public void TestNoAdjustment() => CreateModTest(new ModTestCaseData(new OsuModDifficultyAdjust())
        {
            Autoplay = true,
            PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
        });

        [Test]
        public void TestCircleSize10() => CreateModTest(new ModTestCaseData(new OsuModDifficultyAdjust { CircleSize = { Value = 10 } })
        {
            Autoplay = true,
            PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
        });

        [Test]
        public void TestApproachRate10() => CreateModTest(new ModTestCaseData(new OsuModDifficultyAdjust { ApproachRate = { Value = 10 } })
        {
            Autoplay = true,
            PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
        });

        protected override TestPlayer CreateReplayPlayer(Score score, bool allowFail) => new ScoreAccessibleTestPlayer(score, allowFail);

        private class ScoreAccessibleTestPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreAccessibleTestPlayer(Score score, bool allowFail)
                : base(score, allowFail)
            {
            }
        }
    }
}
