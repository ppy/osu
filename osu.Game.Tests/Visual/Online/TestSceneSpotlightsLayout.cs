// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneSpotlightsLayout : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneSpotlightsLayout()
        {
            var ruleset = new Bindable<RulesetInfo>(new OsuRuleset().RulesetInfo);

            Add(new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Width = 0.8f,
                Child = new SpotlightsLayout
                {
                    Ruleset = { BindTarget = ruleset }
                }
            });

            AddStep("Osu ruleset", () => ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("Mania ruleset", () => ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddStep("Taiko ruleset", () => ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("Catch ruleset", () => ruleset.Value = new CatchRuleset().RulesetInfo);
        }
    }
}
