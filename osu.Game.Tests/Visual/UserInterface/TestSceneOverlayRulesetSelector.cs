// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
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

        [Test]
        public void TestUserPreferredRuleset()
        {
            OverlayRulesetSelector localSelector = null;

            AddStep("Set osu! preferred ruleset", () => API.LocalUser.Value.PlayMode = OsuRuleset.SHORT_NAME);
            AddStep("load overlay ruleset selector", () => Child = new ColourProvidedContainer(OverlayColourScheme.Red, localSelector = new OverlayRulesetSelector()));
            AddAssert("Check osu! selected", () => localSelector.Current.Value.Equals(new OsuRuleset().RulesetInfo));

            AddStep("Set osu!taiko preferred ruleset", () => API.LocalUser.Value.PlayMode = TaikoRuleset.SHORT_NAME);
            AddStep("load overlay ruleset selector", () => Child = new ColourProvidedContainer(OverlayColourScheme.Red, localSelector = new OverlayRulesetSelector()));
            AddAssert("Check osu!taiko selected", () => localSelector.Current.Value.Equals(new TaikoRuleset().RulesetInfo));

            AddStep("Set osu!catch preferred ruleset", () => API.LocalUser.Value.PlayMode = CatchRuleset.SHORT_NAME);
            AddStep("load overlay ruleset selector", () => Child = new ColourProvidedContainer(OverlayColourScheme.Red, localSelector = new OverlayRulesetSelector()));
            AddAssert("Check osu!catch selected", () => localSelector.Current.Value.Equals(new CatchRuleset().RulesetInfo));

            AddStep("Set osu!mania preferred ruleset", () => API.LocalUser.Value.PlayMode = ManiaRuleset.SHORT_NAME);
            AddStep("load overlay ruleset selector", () => Child = new ColourProvidedContainer(OverlayColourScheme.Red, localSelector = new OverlayRulesetSelector()));
            AddAssert("Check osu!mania selected", () => localSelector.Current.Value.Equals(new ManiaRuleset().RulesetInfo));
        }
    }
}
