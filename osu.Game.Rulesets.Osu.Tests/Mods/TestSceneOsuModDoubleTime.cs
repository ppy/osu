// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModDoubleTime : ModSandboxTestScene
    {
        public TestSceneOsuModDoubleTime()
            : base(new OsuRuleset())
        {
        }

        [TestCase(0.5)]
        [TestCase(1.01)]
        [TestCase(1.5)]
        [TestCase(2)]
        [TestCase(5)]
        public void TestDefaultRate(double rate) => CreateModTest(new ModTestCaseData("1.5x", new OsuModDoubleTime { SpeedChange = { Value = rate } })
        {
            PassCondition = () => ((ScoreAccessibleTestPlayer)Player).ScoreProcessor.JudgedHits >= 2
        });

        protected override TestPlayer CreateReplayPlayer(Score score) => new ScoreAccessibleTestPlayer(score);

        private class ScoreAccessibleTestPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreAccessibleTestPlayer(Score score)
                : base(score)
            {
            }
        }
    }
}
