// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanel : OsuTestScene
    {
        private ScorePanel panel;

        [Test]
        public void TestDRank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.5, Rank = ScoreRank.D };

            addPanelStep(score);
        }

        [Test]
        public void TestCRank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.75, Rank = ScoreRank.C };

            addPanelStep(score);
        }

        [Test]
        public void TestBRank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.85, Rank = ScoreRank.B };

            addPanelStep(score);
        }

        [Test]
        public void TestARank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.925, Rank = ScoreRank.A };

            addPanelStep(score);
        }

        [Test]
        public void TestSRank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.975, Rank = ScoreRank.S };

            addPanelStep(score);
        }

        [Test]
        public void TestAlmostSSRank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.9999, Rank = ScoreRank.S };

            addPanelStep(score);
        }

        [Test]
        public void TestSSRank()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 1, Rank = ScoreRank.X };

            addPanelStep(score);
        }

        [Test]
        public void TestAllHitResults()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Statistics = { [HitResult.Perfect] = 350, [HitResult.Ok] = 200 } };

            addPanelStep(score);
        }

        [Test]
        public void TestContractedPanel()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.925, Rank = ScoreRank.A };

            addPanelStep(score, PanelState.Contracted);
        }

        [Test]
        public void TestExpandAndContract()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.925, Rank = ScoreRank.A };

            addPanelStep(score, PanelState.Contracted);
            AddWaitStep("wait for transition", 10);

            AddStep("expand panel", () => panel.State = PanelState.Expanded);
            AddWaitStep("wait for transition", 10);

            AddStep("contract panel", () => panel.State = PanelState.Contracted);
            AddWaitStep("wait for transition", 10);
        }

        [Test]
        public void TestSceneTrackingScorePanel()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo) { Accuracy = 0.925, Rank = ScoreRank.A };

            addPanelStep(score, PanelState.Contracted);

            AddStep("enable tracking", () =>
            {
                panel.Anchor = Anchor.CentreLeft;
                panel.Origin = Anchor.CentreLeft;
                panel.Tracking = true;

                Add(panel.CreateTrackingComponent().With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                }));
            });

            assertTracking(true);

            AddStep("expand panel", () => panel.State = PanelState.Expanded);
            AddWaitStep("wait for transition", 2);
            assertTracking(true);

            AddStep("stop tracking", () => panel.Tracking = false);
            assertTracking(false);

            AddStep("start tracking", () => panel.Tracking = true);
            assertTracking(true);
        }

        private void addPanelStep(ScoreInfo score, PanelState state = PanelState.Expanded) => AddStep("add panel", () =>
        {
            Child = panel = new ScorePanel(score)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                State = state
            };
        });

        private void assertTracking(bool tracking) => AddAssert($"{(tracking ? "is" : "is not")} tracking", () =>
            Precision.AlmostEquals(panel.ScreenSpaceDrawQuad.TopLeft, panel.CreateTrackingComponent().ScreenSpaceDrawQuad.TopLeft) == tracking
            && Precision.AlmostEquals(panel.ScreenSpaceDrawQuad.BottomRight, panel.CreateTrackingComponent().ScreenSpaceDrawQuad.BottomRight) == tracking);
    }
}
