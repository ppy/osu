// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanel : OsuTestScene
    {
        private ScorePanel panel;

        [Test]
        public void TestDRank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.5;
            score.Rank = ScoreRank.D;

            addPanelStep(score);
        }

        [Test]
        public void TestCRank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.75;
            score.Rank = ScoreRank.C;

            addPanelStep(score);
        }

        [Test]
        public void TestBRank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.85;
            score.Rank = ScoreRank.B;

            addPanelStep(score);
        }

        [Test]
        public void TestARank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.925;
            score.Rank = ScoreRank.A;

            addPanelStep(score);
        }

        [Test]
        public void TestSRank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.975;
            score.Rank = ScoreRank.S;

            addPanelStep(score);
        }

        [Test]
        public void TestAlmostSSRank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.9999;
            score.Rank = ScoreRank.S;

            addPanelStep(score);
        }

        [Test]
        public void TestSSRank()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 1;
            score.Rank = ScoreRank.X;

            addPanelStep(score);
        }

        [Test]
        public void TestAllHitResults()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Statistics[HitResult.Perfect] = 350;
            score.Statistics[HitResult.Ok] = 200;

            addPanelStep(score);
        }

        [Test]
        public void TestContractedPanel()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.925;
            score.Rank = ScoreRank.A;

            addPanelStep(score, PanelState.Contracted);
        }

        [Test]
        public void TestExpandAndContract()
        {
            var score = TestResources.CreateTestScoreInfo();
            score.Accuracy = 0.925;
            score.Rank = ScoreRank.A;

            addPanelStep(score, PanelState.Contracted);
            AddWaitStep("wait for transition", 10);

            AddStep("expand panel", () => panel.State = PanelState.Expanded);
            AddWaitStep("wait for transition", 10);

            AddStep("contract panel", () => panel.State = PanelState.Contracted);
            AddWaitStep("wait for transition", 10);
        }

        private void addPanelStep(ScoreInfo score, PanelState state = PanelState.Expanded) => AddStep("add panel", () =>
        {
            Child = panel = new ScorePanel(score, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                State = state
            };
        });
    }
}
