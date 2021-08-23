// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets.Catch;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        private TestRankingsOverlay rankingsOverlay;

        private readonly Bindable<Country> countryBindable = new Bindable<Country>();
        private readonly Bindable<RankingsScope> scope = new Bindable<RankingsScope>();

        [SetUp]
        public void SetUp() => Schedule(loadRankingsOverlay);

        [Test]
        public void TestParentRulesetCopiedOnInitialShow()
        {
            AddStep("set ruleset", () => Ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("reload rankings overlay", loadRankingsOverlay);
            AddAssert("catch ruleset selected", () => Ruleset.Value.Equals(new CatchRuleset().RulesetInfo));
        }

        [Test]
        public void TestFlagScopeDependency()
        {
            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddAssert("Check country is Null", () => countryBindable.Value == null);
            AddStep("Set country", () => countryBindable.Value = us_country);
            AddAssert("Check scope is Performance", () => scope.Value == RankingsScope.Performance);
        }

        [Test]
        public void TestShowCountry()
        {
            AddStep("Show US", () => rankingsOverlay.ShowCountry(us_country));
        }

        private void loadRankingsOverlay()
        {
            Child = rankingsOverlay = new TestRankingsOverlay
            {
                Country = { BindTarget = countryBindable },
                Header = { Current = { BindTarget = scope } },
                State = { Value = Visibility.Visible },
            };
        }

        private static readonly Country us_country = new Country
        {
            FlagName = "US",
            FullName = "United States"
        };

        private class TestRankingsOverlay : RankingsOverlay
        {
            public new Bindable<Country> Country => base.Country;
        }
    }
}
