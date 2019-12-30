// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Taiko;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsOverlayHeader : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankingsOverlayHeader),
            typeof(CountryFilter)
        };

        public TestSceneRankingsOverlayHeader()
        {
            var ruleset = new Bindable<RulesetInfo>();
            var scope = new Bindable<RankingsScope>();
            var countryBindable = new Bindable<Country>();

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

            Add(new RankingsOverlayHeader
            {
                Ruleset = { BindTarget = ruleset },
                Current = { BindTarget = scope },
                Country = { BindTarget = countryBindable }
            });

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
