// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Framework.Bindables;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using NUnit.Framework;
using osu.Game.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOverlayRulesetSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OverlayRulesetSelector),
            typeof(OverlayRulesetTabItem),
        };

        private readonly OverlayRulesetSelector selector;
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public TestSceneOverlayRulesetSelector()
        {
            Add(selector = new OverlayRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = ruleset,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            selector.AccentColour = colours.Lime;
        }

        [Test]
        public void TestSelection()
        {
            var osuRuleset = new OsuRuleset().RulesetInfo;
            var maniaRuleset = new ManiaRuleset().RulesetInfo;
            var taikoRuleset = new TaikoRuleset().RulesetInfo;
            var catchRuleset = new CatchRuleset().RulesetInfo;

            AddStep("Select osu!", () => ruleset.Value = osuRuleset);
            AddAssert("Check osu! selected", () => selector.Current.Value == osuRuleset);

            AddStep("Select mania", () => ruleset.Value = maniaRuleset);
            AddAssert("Check mania selected", () => selector.Current.Value == maniaRuleset);

            AddStep("Select taiko", () => ruleset.Value = taikoRuleset);
            AddAssert("Check taiko selected", () => selector.Current.Value == taikoRuleset);

            AddStep("Select catch", () => ruleset.Value = catchRuleset);
            AddAssert("Check catch selected", () => selector.Current.Value == catchRuleset);
        }
    }
}
