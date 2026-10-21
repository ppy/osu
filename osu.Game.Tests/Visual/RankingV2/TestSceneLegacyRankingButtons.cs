// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Legacy;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneLegacyRankingButtons : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Test]
        public void TestRetryButton()
            => AddStep("create button", () => SetContents(_ => new LegacyRankingRetryButton { Action = () => { } }));

        [Test]
        public void TestWatchReplayButton()
            => AddStep("create button", () => SetContents(_ => new LegacyRankingWatchReplayButton { Action = () => { } }));
    }
}
