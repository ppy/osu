// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Taiko;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsHeader : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankingsHeader),
            typeof(CountryFilter)
        };

        public TestSceneRankingsHeader()
        {
            var countryBindable = new Bindable<Country>();
            var ruleset = new Bindable<RulesetInfo>();
            var scope = new Bindable<RankingsScope>();

            Add(new RankingsHeader(OverlayColourScheme.Green)
            {
                Current = { BindTarget = scope },
                Country = { BindTarget = countryBindable },
                Ruleset = { BindTarget = ruleset },
                Spotlights = new[]
                {
                    new Spotlight
                    {
                        Id = 1,
                        Text = "Spotlight 1"
                    },
                    new Spotlight
                    {
                        Id = 2,
                        Text = "Spotlight 2"
                    },
                    new Spotlight
                    {
                        Id = 3,
                        Text = "Spotlight 3"
                    }
                }
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

            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddStep("Set scope to Country", () => scope.Value = RankingsScope.Country);
            AddStep("Set scope to Performance", () => scope.Value = RankingsScope.Performance);
            AddStep("Set scope to Spotlights", () => scope.Value = RankingsScope.Spotlights);
            AddStep("Set ruleset to Taiko", () => ruleset.Value = new TaikoRuleset().RulesetInfo);

            AddStep("Set country", () => countryBindable.Value = country);
            AddStep("Set null country", () => countryBindable.Value = null);
            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddStep("Set country with no flag", () => countryBindable.Value = unknownCountry);
        }
    }
}
