// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneRankingsHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneRankingsHeader()
        {
            var countryBindable = new Bindable<CountryCode>();
            var ruleset = new Bindable<RulesetInfo>();
            var scope = new Bindable<RankingsScope>();

            Add(new RankingsOverlayHeader
            {
                Current = { BindTarget = scope },
                Country = { BindTarget = countryBindable },
                Ruleset = { BindTarget = ruleset }
            });

            const CountryCode country = CountryCode.BY;
            const CountryCode unknown_country = CountryCode.CK;

            AddStep("Set country", () => countryBindable.Value = country);
            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddStep("Set country with no flag", () => countryBindable.Value = unknown_country);
        }
    }
}
