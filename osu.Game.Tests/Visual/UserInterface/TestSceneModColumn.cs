// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Utils;
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
                    Origin = Anchor.Centre,
                    AvailableMods = getExampleModsFor(modType)
                }
            });
        }

        [Test]
        public void TestMultiSelection()
        {
            ModColumn column = null!;
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new ModColumn(ModType.DifficultyIncrease, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AvailableMods = getExampleModsFor(ModType.DifficultyIncrease)
                }
            });

            AddUntilStep("wait for panel load", () => column.IsLoaded && column.ItemsLoaded);

            clickToggle();
            AddUntilStep("all panels selected", () => this.ChildrenOfType<ModPanel>().All(panel => panel.Active.Value));

            clickToggle();
            AddUntilStep("all panels deselected", () => this.ChildrenOfType<ModPanel>().All(panel => !panel.Active.Value));

            AddStep("manually activate all panels", () => this.ChildrenOfType<ModPanel>().ForEach(panel => panel.Active.Value = true));
            AddUntilStep("checkbox selected", () => this.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("deselect first panel", () => this.ChildrenOfType<ModPanel>().First().Active.Value = false);
            AddUntilStep("checkbox not selected", () => !this.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

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
            TestModColumn column = null!;

            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new TestModColumn(ModType.Fun, true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AvailableMods = getExampleModsFor(ModType.Fun)
                }
            });

            AddStep("set filter", () => setFilter(mod => mod.Name.Contains("Wind", StringComparison.CurrentCultureIgnoreCase)));
            AddUntilStep("two panels visible", () => column.ChildrenOfType<ModPanel>().Count(panel => !panel.Filtered.Value) == 2);

            clickToggle();
            AddUntilStep("wait for animation", () => !column.SelectionAnimationRunning);
            AddAssert("only visible items selected", () => column.ChildrenOfType<ModPanel>().Where(panel => panel.Active.Value).All(panel => !panel.Filtered.Value));

            AddStep("unset filter", () => setFilter(null));
            AddUntilStep("all panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => !panel.Filtered.Value));
            AddAssert("checkbox not selected", () => !column.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("set filter", () => setFilter(mod => mod.Name.Contains("Wind", StringComparison.CurrentCultureIgnoreCase)));
            AddUntilStep("two panels visible", () => column.ChildrenOfType<ModPanel>().Count(panel => !panel.Filtered.Value) == 2);
            AddAssert("checkbox selected", () => column.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("filter out everything", () => setFilter(_ => false));
            AddUntilStep("no panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => panel.Filtered.Value));
            AddUntilStep("checkbox hidden", () => !column.ChildrenOfType<OsuCheckbox>().Single().IsPresent);

            AddStep("inset filter", () => setFilter(null));
            AddUntilStep("all panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => !panel.Filtered.Value));
            AddUntilStep("checkbox visible", () => column.ChildrenOfType<OsuCheckbox>().Single().IsPresent);

            void clickToggle() => AddStep("click toggle", () =>
            {
                var checkbox = this.ChildrenOfType<OsuCheckbox>().Single();
                InputManager.MoveMouseTo(checkbox);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestKeyboardSelection()
        {
            ModColumn column = null!;
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new ModColumn(ModType.DifficultyReduction, true, new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AvailableMods = getExampleModsFor(ModType.DifficultyReduction)
                }
            });

            AddUntilStep("wait for panel load", () => column.IsLoaded && column.ItemsLoaded);

            AddStep("press W", () => InputManager.Key(Key.W));
            AddAssert("NF panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("press W again", () => InputManager.Key(Key.W));
            AddAssert("NF panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("set filter to NF", () => setFilter(mod => mod.Acronym == "NF"));

            AddStep("press W", () => InputManager.Key(Key.W));
            AddAssert("NF panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("press W again", () => InputManager.Key(Key.W));
            AddAssert("NF panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("filter out everything", () => setFilter(_ => false));

            AddStep("press W", () => InputManager.Key(Key.W));
            AddAssert("NF panel not selected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("clear filter", () => setFilter(null));
        }

        private void setFilter(Func<Mod, bool>? filter)
        {
            foreach (var modState in this.ChildrenOfType<ModColumn>().Single().AvailableMods)
                modState.Filtered.Value = filter?.Invoke(modState.Mod) == false;
        }

        private class TestModColumn : ModColumn
        {
            public new bool SelectionAnimationRunning => base.SelectionAnimationRunning;

            public TestModColumn(ModType modType, bool allowBulkSelection)
                : base(modType, allowBulkSelection)
            {
            }
        }

        private static ModState[] getExampleModsFor(ModType modType)
        {
            return new OsuRuleset().GetModsFor(modType)
                                   .SelectMany(ModUtils.FlattenMod)
                                   .Select(mod => new ModState(mod))
                                   .ToArray();
        }
    }
}
