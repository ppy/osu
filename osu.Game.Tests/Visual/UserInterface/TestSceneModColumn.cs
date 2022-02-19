// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneModColumn : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [TestCase(ModType.DifficultyReduction)]
        [TestCase(ModType.DifficultyIncrease)]
        [TestCase(ModType.Conversion)]
        [TestCase(ModType.Automation)]
        [TestCase(ModType.Fun)]
        public void TestBasic(ModType modType)
        {
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = new ModColumn(modType)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            });

            AddStep("change ruleset to osu!", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("change ruleset to taiko", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("change ruleset to catch", () => Ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("change ruleset to mania", () => Ruleset.Value = new ManiaRuleset().RulesetInfo);
        }
    }
}
