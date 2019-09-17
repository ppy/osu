// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsHeader : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DismissableFlag),
            typeof(HeaderTitle),
            typeof(RankingsRulesetSelector),
            typeof(RankingsScopeSelector),
            typeof(RankingsHeader),
        };

        public TestSceneRankingsHeader()
        {
            var countryBindable = new Bindable<Country>();
            var ruleset = new Bindable<RulesetInfo>();
            var scope = new Bindable<RankingsScope>();

            Add(new RankingsHeader
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scope = { BindTarget = scope },
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

            AddStep("Set country", () => countryBindable.Value = country);
            AddAssert("Check scope is Performance", () => scope.Value == RankingsScope.Performance);
            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddAssert("Check country is Null", () => countryBindable.Value == null);
            AddStep("Set country with no flag", () => countryBindable.Value = unknownCountry);
        }
    }
}
