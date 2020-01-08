// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsCountryFilter : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CountryFilter),
        };

        public TestSceneRankingsCountryFilter()
        {
            var countryBindable = new Bindable<Country>();

            Add(new CountryFilter
            {
                Country = { BindTarget = countryBindable }
            });

            var country = new Country
            {
                FlagName = "BY",
                FullName = "Belarus"
            };
            var unknownCountry = new Country
            {
                FlagName = "CK",
                FullName = "Cook Islands"
            };

            AddStep("Set country", () => countryBindable.Value = country);
            AddStep("Set null country", () => countryBindable.Value = null);
            AddStep("Set country with no flag", () => countryBindable.Value = unknownCountry);
        }
    }
}
