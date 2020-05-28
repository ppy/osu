// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanelList : OsuManualInputManagerTestScene
    {
        private ScoreInfo initialScore;
        private ScorePanelList list;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = list = new ScorePanelList(initialScore = new TestScoreInfo(new OsuRuleset().RulesetInfo))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        });

        [Test]
        public void TestSingleScore()
        {
            assertPanelCentred();
        }

        [Test]
        public void TestAddManyScoresAfter()
        {
            AddStep("add scores", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore - i - 1 });
            });

            assertPanelCentred();
        }

        [Test]
        public void TestAddManyScoresBefore()
        {
            AddStep("add scores", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore + i + 1 });
            });

            assertPanelCentred();
        }

        [Test]
        public void TestAddManyPanelsOnBothSides()
        {
            AddStep("add scores after", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore - i - 1 });

                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore + i + 1 });
            });

            assertPanelCentred();
        }

        [Test]
        public void TestNullScore()
        {
            AddStep("create panel with null score", () =>
            {
                Child = list = new ScorePanelList(null)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddStep("add many panels", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore - i - 1 });

                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore + i + 1 });
            });

            AddWaitStep("wait for panel animation", 5);

            AddAssert("no panel selected", () => list.ChildrenOfType<ScorePanel>().All(p => p.State != PanelState.Expanded));

            AddStep("expand second panel", () =>
            {
                var expandedPanel = list.ChildrenOfType<ScorePanel>().OrderBy(p => p.DrawPosition.X).ElementAt(1);
                InputManager.MoveMouseTo(expandedPanel);
                InputManager.Click(MouseButton.Left);
            });

            assertPanelCentred();
        }

        private void assertPanelCentred() => AddUntilStep("expanded panel centred", () =>
        {
            var expandedPanel = list.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
            return Precision.AlmostEquals(expandedPanel.ScreenSpaceDrawQuad.Centre.X, list.ScreenSpaceDrawQuad.Centre.X, 1);
        });
    }
}
