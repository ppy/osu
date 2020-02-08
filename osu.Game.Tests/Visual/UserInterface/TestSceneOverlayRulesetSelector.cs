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
using osu.Framework.Graphics.Containers;
using osuTK;
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
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new[]
                {
                    new ColourProvidedContainer(OverlayColourScheme.Green, selector = new OverlayRulesetSelector { Current = ruleset }),
                    new ColourProvidedContainer(OverlayColourScheme.Blue, new OverlayRulesetSelector { Current = ruleset }),
                    new ColourProvidedContainer(OverlayColourScheme.Orange, new OverlayRulesetSelector { Current = ruleset }),
                    new ColourProvidedContainer(OverlayColourScheme.Pink, new OverlayRulesetSelector { Current = ruleset }),
                    new ColourProvidedContainer(OverlayColourScheme.Purple, new OverlayRulesetSelector { Current = ruleset }),
                    new ColourProvidedContainer(OverlayColourScheme.Red, new OverlayRulesetSelector { Current = ruleset }),
                }
            });
        }

        private class ColourProvidedContainer : Container
        {
            [Cached]
            private readonly OverlayColourProvider colourProvider;

            public ColourProvidedContainer(OverlayColourScheme colourScheme, OverlayRulesetSelector rulesetSelector)
            {
                colourProvider = new OverlayColourProvider(colourScheme);
                AutoSizeAxes = Axes.Both;
                Add(rulesetSelector);
            }
        }

        [Test]
        public void TestSelection()
        {
            AddStep("Select osu!", () => ruleset.Value = new OsuRuleset().RulesetInfo);
            AddAssert("Check osu! selected", () => selector.Current.Value.Equals(new OsuRuleset().RulesetInfo));

            AddStep("Select mania", () => ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddAssert("Check mania selected", () => selector.Current.Value.Equals(new ManiaRuleset().RulesetInfo));

            AddStep("Select taiko", () => ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddAssert("Check taiko selected", () => selector.Current.Value.Equals(new TaikoRuleset().RulesetInfo));

            AddStep("Select catch", () => ruleset.Value = new CatchRuleset().RulesetInfo);
            AddAssert("Check catch selected", () => selector.Current.Value.Equals(new CatchRuleset().RulesetInfo));
        }
    }
}
