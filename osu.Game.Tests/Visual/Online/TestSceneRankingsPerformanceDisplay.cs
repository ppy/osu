// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings.Displays;
using osu.Game.Users;
using NUnit.Framework;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsPerformanceDisplay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        private PerformanceRankingsDisplay display;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = display = new PerformanceRankingsDisplay().With(d => d.Current.Value = new OsuRuleset().RulesetInfo)
            };
        });

        [Test]
        public void TestCountry()
        {
            AddStep("Show US", () =>
            {
                display.Country.Value = new Country
                {
                    FlagName = "US",
                    FullName = "United States"
                };
            });
        }
    }
}
