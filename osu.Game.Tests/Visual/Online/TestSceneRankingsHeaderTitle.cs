// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsHeaderTitle : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DismissableFlag),
            typeof(HeaderTitle),
        };

        public TestSceneRankingsHeaderTitle()
        {
            var countryBindable = new Bindable<Country>();
            var scope = new Bindable<RankingsScope>();

            Add(new HeaderTitle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Country = { BindTarget = countryBindable },
                Scope = { BindTarget = scope },
            });

            var countryA = new Country
            {
                FlagName = "BY",
                FullName = "Belarus"
            };

            var countryB = new Country
            {
                FlagName = "US",
                FullName = "United States"
            };

            AddStep("Set country", () => countryBindable.Value = countryA);
            AddAssert("Check scope is Performance", () => scope.Value == RankingsScope.Performance);
            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddAssert("Check country is Null", () => countryBindable.Value == null);

            AddStep("Set country 1", () => countryBindable.Value = countryA);
            AddStep("Set country 2", () => countryBindable.Value = countryB);
            AddStep("Set null country", () => countryBindable.Value = null);
            AddStep("Set scope to Performance", () => scope.Value = RankingsScope.Performance);
            AddStep("Set scope to Spotlights", () => scope.Value = RankingsScope.Spotlights);
            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddStep("Set scope to Country", () => scope.Value = RankingsScope.Country);
        }
    }
}
