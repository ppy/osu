// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Legacy;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneLegacyRankingPerfectIndicator : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => score.Value = createScore(perfect: true));
            AddStep("create content", () => SetContents(_ => new LegacyRankingPerfectIndicator()));

            AddToggleStep("toggle outline", t =>
            {
                foreach (var outline in this.ChildrenOfType<OutlineBox>())
                    outline.Alpha = t ? 1 : 0;
            });
            AddStep("begin animation", () =>
            {
                foreach (var indicator in this.ChildrenOfType<LegacyRankingPerfectIndicator>())
                    indicator.StartAnimating(Time.Current);
            });
            AddStep("cancel animation", () =>
            {
                foreach (var indicator in this.ChildrenOfType<LegacyRankingPerfectIndicator>())
                    indicator.FinishAnimating();
            });
            AddToggleStep("toggle perfect", perfect =>
            {
                if (IsLoaded)
                    score.Value = createScore(perfect);
            });
        }

        private static ScoreInfo createScore(bool perfect)
        {
            var score = new ScoreInfo
            {
                Statistics =
                {
                    [HitResult.Great] = 100,
                },
                MaximumStatistics =
                {
                    [HitResult.Great] = 100,
                },
                MaxCombo = perfect ? 100 : 99,
            };
            return score;
        }
    }
}
