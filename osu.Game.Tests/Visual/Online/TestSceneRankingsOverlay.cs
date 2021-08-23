// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using NUnit.Framework;
using osu.Game.Users;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached(typeof(RankingsOverlay))]
        private readonly RankingsOverlay rankingsOverlay;

        private readonly Bindable<Country> countryBindable = new Bindable<Country>();
        private readonly Bindable<RankingsScope> scope = new Bindable<RankingsScope>();

        public TestSceneRankingsOverlay()
        {
            Add(rankingsOverlay = new TestRankingsOverlay
            {
                Country = { BindTarget = countryBindable },
                Header = { Current = { BindTarget = scope } },
            });
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", rankingsOverlay.Show);
        }

        [Test]
        public void TestRulesetResetsOnNoFilters()
        {
            AddStep("Set different ruleset", () => rankingsOverlay.ChildrenOfType<RulesetSelector>().Single().SwitchTab(1));
            AddAssert("Ruleset not default", () => !rankingsOverlay.Header.Ruleset.IsDefault);
            AddStep("Hide overlay", () => rankingsOverlay.Hide());
            AddUntilStep("Wait for hide", () => !rankingsOverlay.IsPresent);

            AddStep("Show overlay again", () => rankingsOverlay.Show());
            AddAssert("Ruleset reverted", () => rankingsOverlay.Header.Ruleset.IsDefault);
        }

        [Test]
        public void TestRulesetDoesntResetOnCountryFilter()
        {
            AddStep("Set different ruleset", () => rankingsOverlay.ChildrenOfType<RulesetSelector>().Single().SwitchTab(1));
            AddAssert("Ruleset not default", () => !rankingsOverlay.Header.Ruleset.IsDefault);
            AddStep("Set country filter", () => countryBindable.Value = us_country);
            AddStep("Hide overlay", () => rankingsOverlay.Hide());
            AddUntilStep("Wait for hide", () => !rankingsOverlay.IsPresent);

            AddStep("Show overlay again", () => rankingsOverlay.Show());
            AddAssert("Ruleset not reverted", () => !rankingsOverlay.Header.Ruleset.IsDefault);
        }

        [Test]
        public void TestRulesetDoesntResetOnScopeChange()
        {
            AddStep("Set ruleset to osu!taiko", () => rankingsOverlay.Header.Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddAssert("Ruleset not default", () => !rankingsOverlay.Header.Ruleset.IsDefault);
            AddStep("Set different scope", () => scope.Value = RankingsScope.Score);
            AddStep("Hide overlay", () => rankingsOverlay.Hide());
            AddUntilStep("Wait for hide", () => !rankingsOverlay.IsPresent);

            AddStep("Show overlay again", () => rankingsOverlay.Show());
            AddAssert("Ruleset not reverted", () => !rankingsOverlay.Header.Ruleset.IsDefault);
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

        [Test]
        public void TestHide()
        {
            AddStep("Hide", rankingsOverlay.Hide);
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
