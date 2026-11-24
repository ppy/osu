// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Legacy;
using osuTK;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneLegacyRankingGrade : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => score.Value = createScore(ScoreRank.A));
            AddStep("create content", () => SetContents(_ => new LegacyRankingGrade
            {
                Scale = new Vector2(0.8f),
            }));

            AddToggleStep("toggle outline", t =>
            {
                foreach (var outline in this.ChildrenOfType<OutlineBox>())
                    outline.Alpha = t ? 1 : 0;
            });
            AddStep("begin animation", () =>
            {
                foreach (var indicator in this.ChildrenOfType<LegacyRankingGrade>())
                    indicator.StartAnimating(Time.Current);
            });
            AddStep("cancel animation", () =>
            {
                foreach (var indicator in this.ChildrenOfType<LegacyRankingGrade>())
                    indicator.FinishAnimating();
            });

            foreach (var scoreRank in Enum.GetValues<ScoreRank>())
                AddStep($"rank {scoreRank}", () => score.Value = createScore(scoreRank));
        }

        private static ScoreInfo createScore(ScoreRank rank)
        {
            var score = new ScoreInfo
            {
                Rank = rank,
            };
            return score;
        }
    }
}
