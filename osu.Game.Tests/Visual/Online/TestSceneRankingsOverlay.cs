// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings;
using osu.Game.Overlays.Rankings.Tables;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneRankingsOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankingsOverlay),
            typeof(RankingsHeader),
            typeof(PerformanceTable),
            typeof(ScoresTable),
            typeof(CountriesTable),
            typeof(TableRowBackground),
        };

        [Cached]
        private readonly RankingsOverlay overlay;

        public TestSceneRankingsOverlay()
        {
            Add(overlay = new RankingsOverlay());

            AddStep("Toggle visibility", overlay.ToggleVisibility);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Show US", () => overlay.ShowCountry(new Country
            {
                FlagName = "US",
                FullName = "United States"
            }));
        }
    }
}
