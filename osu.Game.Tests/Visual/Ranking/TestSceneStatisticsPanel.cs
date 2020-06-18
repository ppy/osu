// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneStatisticsPanel : OsuTestScene
    {
        [Test]
        public void TestScore()
        {
            var score = new TestScoreInfo(new OsuRuleset().RulesetInfo)
            {
                HitEvents = TestSceneTimingDistributionGraph.CreateDistributedHitEvents().Cast<object>().ToList(),
            };

            loadPanel(score);
        }

        private void loadPanel(ScoreInfo score) => AddStep("load panel", () =>
        {
            Child = new StatisticsPanel(score);
        });
    }
}
