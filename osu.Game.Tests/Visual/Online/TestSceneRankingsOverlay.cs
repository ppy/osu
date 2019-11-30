// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays.Rankings.Tables;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using NUnit.Framework;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PerformanceTable),
            typeof(ScoresTable),
            typeof(CountriesTable),
            typeof(TableRowBackground),
            typeof(UserBasedTable),
            typeof(RankingsTable<>),
            typeof(RankingsOverlay)
        };

        [Cached]
        private RankingsOverlay rankingsOverlay;

        public TestSceneRankingsOverlay()
        {
            Add(rankingsOverlay = new RankingsOverlay());
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", rankingsOverlay.Show);
        }

        [Test]
        public void TestShowCountry()
        {
            AddStep("Show US", () => rankingsOverlay.ShowCountry(new Country
            {
                FlagName = "US",
                FullName = "United States"
            }));
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", rankingsOverlay.Hide);
        }
    }
}
