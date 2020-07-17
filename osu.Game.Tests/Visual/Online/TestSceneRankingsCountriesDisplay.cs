// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings.Displays;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsCountriesDisplay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneRankingsCountriesDisplay()
        {
            CountriesDisplay display;

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = display = new CountriesDisplay()
            };

            display.Ruleset.Value = new OsuRuleset().RulesetInfo;
        }
    }
}
