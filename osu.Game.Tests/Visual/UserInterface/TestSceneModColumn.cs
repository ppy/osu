// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneModColumn : OsuManualInputManagerTestScene
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
                Child = new ModColumn(modType, false)
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

        [Test]
        public void TestMultiSelection()
        {
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = new ModColumn(ModType.DifficultyIncrease, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            });

            clickToggle();
            AddUntilStep("all panels selected", () => this.ChildrenOfType<ModPanel>().All(panel => panel.Active.Value));

            clickToggle();
            AddUntilStep("all panels deselected", () => this.ChildrenOfType<ModPanel>().All(panel => !panel.Active.Value));

            AddStep("manually activate all panels", () => this.ChildrenOfType<ModPanel>().ForEach(panel => panel.Active.Value = true));
            AddUntilStep("checkbox selected", () => this.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("deselect first panel", () => this.ChildrenOfType<ModPanel>().First().Active.Value = false);
            AddUntilStep("checkbox selected", () => !this.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            void clickToggle() => AddStep("click toggle", () =>
            {
                var checkbox = this.ChildrenOfType<OsuCheckbox>().Single();
                InputManager.MoveMouseTo(checkbox);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestFiltering()
        {
            TestModColumn column = null;

            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new TestModColumn(ModType.Fun, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            });

            AddStep("set filter", () => column.Filter = mod => mod.Name.Contains("Wind", StringComparison.CurrentCultureIgnoreCase));
            AddUntilStep("two panels visible", () => column.ChildrenOfType<ModPanel>().Count(panel => !panel.Filtered.Value) == 2);

            clickToggle();
            AddUntilStep("wait for animation", () => !column.SelectionAnimationRunning);
            AddAssert("only visible items selected", () => column.ChildrenOfType<ModPanel>().Where(panel => panel.Active.Value).All(panel => !panel.Filtered.Value));

            AddStep("unset filter", () => column.Filter = null);
            AddUntilStep("all panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => !panel.Filtered.Value));
            AddAssert("checkbox not selected", () => !column.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("set filter", () => column.Filter = mod => mod.Name.Contains("Wind", StringComparison.CurrentCultureIgnoreCase));
            AddUntilStep("two panels visible", () => column.ChildrenOfType<ModPanel>().Count(panel => !panel.Filtered.Value) == 2);
            AddAssert("checkbox selected", () => column.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            void clickToggle() => AddStep("click toggle", () =>
            {
                var checkbox = this.ChildrenOfType<OsuCheckbox>().Single();
                InputManager.MoveMouseTo(checkbox);
                InputManager.Click(MouseButton.Left);
            });
        }

        private class TestModColumn : ModColumn
        {
            public new bool SelectionAnimationRunning => base.SelectionAnimationRunning;

            public TestModColumn(ModType modType, bool allowBulkSelection)
                : base(modType, allowBulkSelection)
            {
            }
        }
    }
}
