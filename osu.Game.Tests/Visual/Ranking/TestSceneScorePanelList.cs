// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanelList : OsuManualInputManagerTestScene
    {
        private ScorePanelList list;

        [Test]
        public void TestEmptyList()
        {
            createListStep(() => new ScorePanelList());
        }

        [Test]
        public void TestEmptyListWithSelectedScore()
        {
            createListStep(() => new ScorePanelList
            {
                SelectedScore = { Value = new TestScoreInfo(new OsuRuleset().RulesetInfo) }
            });
        }

        [Test]
        public void TestAddPanelAfterSelectingScore()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo);

            createListStep(() => new ScorePanelList
            {
                SelectedScore = { Value = score }
            });

            AddStep("add panel", () => list.AddScore(score));

            assertScoreState(score, true);
            assertPanelCentred();
        }

        [Test]
        public void TestAddPanelBeforeSelectingScore()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo);

            createListStep(() => new ScorePanelList());

            AddStep("add panel", () => list.AddScore(score));

            assertScoreState(score, false);
            assertPanelCentred();

            AddStep("select score", () => list.SelectedScore.Value = score);

            assertScoreState(score, true);
            assertPanelCentred();
        }

        [Test]
        public void TestAddManyScoresAfter()
        {
            var initialScore = new TestScoreInfo(new OsuRuleset().RulesetInfo);

            createListStep(() => new ScorePanelList());

            AddStep("add initial panel and select", () =>
            {
                list.AddScore(initialScore);
                list.SelectedScore.Value = initialScore;
            });

            AddStep("add many scores", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore - i - 1 });
            });

            assertScoreState(initialScore, true);
            assertPanelCentred();
        }

        [Test]
        public void TestAddManyScoresBefore()
        {
            var initialScore = new TestScoreInfo(new OsuRuleset().RulesetInfo);

            createListStep(() => new ScorePanelList());

            AddStep("add initial panel and select", () =>
            {
                list.AddScore(initialScore);
                list.SelectedScore.Value = initialScore;
            });

            AddStep("add scores", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore + i + 1 });
            });

            assertScoreState(initialScore, true);
            assertPanelCentred();
        }

        [Test]
        public void TestAddManyPanelsOnBothSides()
        {
            var initialScore = new TestScoreInfo(new OsuRuleset().RulesetInfo);

            createListStep(() => new ScorePanelList());

            AddStep("add initial panel and select", () =>
            {
                list.AddScore(initialScore);
                list.SelectedScore.Value = initialScore;
            });

            AddStep("add scores after", () =>
            {
                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore - i - 1 });

                for (int i = 0; i < 20; i++)
                    list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo) { TotalScore = initialScore.TotalScore + i + 1 });
            });

            assertScoreState(initialScore, true);
            assertPanelCentred();
        }

        private void createListStep(Func<ScorePanelList> creationFunc)
        {
            AddStep("create list", () => Child = list = creationFunc().With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
            }));

            AddUntilStep("wait for load", () => list.IsLoaded);
        }

        private void assertPanelCentred() => AddUntilStep("expanded panel centred", () =>
        {
            var expandedPanel = list.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
            return Precision.AlmostEquals(expandedPanel.ScreenSpaceDrawQuad.Centre.X, list.ScreenSpaceDrawQuad.Centre.X, 1);
        });

        private void assertScoreState(ScoreInfo score, bool expanded)
            => AddUntilStep($"correct score expanded = {expanded}", () => (list.ChildrenOfType<ScorePanel>().Single(p => p.Score == score).State == PanelState.Expanded) == expanded);
    }
}
