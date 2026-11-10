// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Legacy;
using osu.Game.Tests.Visual.Ranking;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneLegacyRankingHealthGraph : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => score.Value = createScore());
            AddStep("create content", () => SetContents(_ => new LegacyRankingHealthGraph()));

            AddToggleStep("toggle outline", t =>
            {
                foreach (var outline in this.ChildrenOfType<OutlineBox>())
                    outline.Alpha = t ? 1 : 0;
            });
            AddStep("begin animation", () =>
            {
                foreach (var graph in this.ChildrenOfType<LegacyRankingHealthGraph>())
                    graph.StartAnimating(Time.Current);
            });
            AddStep("cancel animation", () =>
            {
                foreach (var graph in this.ChildrenOfType<LegacyRankingHealthGraph>())
                    graph.FinishAnimating();
            });

            AddStep("score with empty health graph", () => score.Value = createScore(0));
            AddStep("score with 1 point on health graph", () => score.Value = createScore(1));
            AddStep("score with 2 points on health graph", () => score.Value = createScore(2));
            AddStep("score with very many points on health graph", () => score.Value = createScore(10000));
        }

        private static ScoreInfo createScore(int healthPoints = 30)
        {
            var graph = new List<HealthValue>();

            for (int i = 0; i < healthPoints; ++i)
                graph.Add(new HealthValue(1500 * i, RNG.NextDouble()));

            var score = new ScoreInfo
            {
                HealthGraph = graph,
                HitEvents = TestSceneStatisticsPanel.CreatePositionDistributedHitEvents(),
            };
            return score;
        }
    }
}
