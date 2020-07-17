// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays;
using NUnit.Framework;
using osu.Game.Users;
using osu.Framework.Bindables;
using osu.Game.Overlays.Rankings;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached(typeof(RankingsOverlay))]
        private readonly TestRankingsOverlay rankings;

        public TestSceneRankingsOverlay()
        {
            Add(rankings = new TestRankingsOverlay());
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", rankings.Show);
        }

        [Test]
        public void TestFlagScopeDependency()
        {
            AddStep("Set scope to Score", () => rankings.Scope.Value = RankingsScope.Score);
            AddStep("Show US", () => rankings.ShowCountry(us_country));
            AddAssert("Check scope is Performance", () => rankings.Scope.Value == RankingsScope.Performance);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", rankings.Hide);
        }

        private static readonly Country us_country = new Country
        {
            FlagName = "US",
            FullName = "United States"
        };

        private class TestRankingsOverlay : RankingsOverlay
        {
            public new Bindable<RankingsScope> Scope => base.Scope;
        }
    }
}
