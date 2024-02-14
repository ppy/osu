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
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Mods.Input;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModColumn : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

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
            AddUntilStep("two panels visible", () => column.ChildrenOfType<ModPanel>().Count(panel => panel.Visible) == 2);

            clickToggle();
            AddUntilStep("wait for animation", () => !column.SelectionAnimationRunning);
            AddAssert("only visible items selected", () => column.ChildrenOfType<ModPanel>().Where(panel => panel.Active.Value).All(panel => panel.Visible));

            AddStep("unset filter", () => setFilter(null));
            AddUntilStep("all panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => panel.Visible));
            AddAssert("checkbox not selected", () => !column.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("set filter", () => setFilter(mod => mod.Name.Contains("Wind", StringComparison.CurrentCultureIgnoreCase)));
            AddUntilStep("two panels visible", () => column.ChildrenOfType<ModPanel>().Count(panel => panel.Visible) == 2);
            AddAssert("checkbox selected", () => column.ChildrenOfType<OsuCheckbox>().Single().Current.Value);

            AddStep("filter out everything", () => setFilter(_ => false));
            AddUntilStep("no panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => !panel.Visible));
            AddUntilStep("checkbox hidden", () => !column.ChildrenOfType<OsuCheckbox>().Single().IsPresent);

            AddStep("inset filter", () => setFilter(null));
            AddUntilStep("all panels visible", () => column.ChildrenOfType<ModPanel>().All(panel => panel.Visible));
            AddUntilStep("checkbox visible", () => column.ChildrenOfType<OsuCheckbox>().Single().IsPresent);

            void clickToggle() => AddStep("click toggle", () =>
            {
                var checkbox = this.ChildrenOfType<OsuCheckbox>().Single();
                InputManager.MoveMouseTo(checkbox);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestSequentialKeyboardSelection()
        {
            AddStep("set sequential hotkey mode", () => configManager.SetValue(OsuSetting.ModSelectHotkeyStyle, ModSelectHotkeyStyle.Sequential));

            ModColumn column = null!;
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new ModColumn(ModType.DifficultyReduction, true)
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
            AddAssert("NF panel not selected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("press Q", () => InputManager.Key(Key.Q));
            AddAssert("NF panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("press Q again", () => InputManager.Key(Key.Q));
            AddAssert("NF panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("filter out everything", () => setFilter(_ => false));

            AddStep("press W", () => InputManager.Key(Key.W));
            AddAssert("NF panel not selected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NF").Active.Value);

            AddStep("clear filter", () => setFilter(null));
        }

        [Test]
        public void TestClassicKeyboardExclusiveSelection()
        {
            AddStep("set classic hotkey mode", () => configManager.SetValue(OsuSetting.ModSelectHotkeyStyle, ModSelectHotkeyStyle.Classic));

            ModColumn column = null!;
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new ModColumn(ModType.DifficultyIncrease, false)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AvailableMods = getExampleModsFor(ModType.DifficultyIncrease)
                }
            });

            AddUntilStep("wait for panel load", () => column.IsLoaded && column.ItemsLoaded);

            AddStep("press A", () => InputManager.Key(Key.A));
            AddAssert("HR panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "HR").Active.Value);

            AddStep("press A again", () => InputManager.Key(Key.A));
            AddAssert("HR panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "HR").Active.Value);

            AddStep("press D", () => InputManager.Key(Key.D));
            AddAssert("DT panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);

            AddStep("press D again", () => InputManager.Key(Key.D));
            AddAssert("DT panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);
            AddAssert("NC panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NC").Active.Value);

            AddStep("press D again", () => InputManager.Key(Key.D));
            AddAssert("DT panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);
            AddAssert("NC panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NC").Active.Value);

            AddStep("press Shift-D", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.D);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            AddAssert("DT panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);
            AddAssert("NC panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NC").Active.Value);

            AddStep("press J", () => InputManager.Key(Key.J));
            AddAssert("no change", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Active.Value).Mod.Acronym == "NC");

            AddStep("filter everything but NC", () => setFilter(mod => mod.Acronym == "NC"));

            AddStep("press A", () => InputManager.Key(Key.A));
            AddAssert("no change", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Active.Value).Mod.Acronym == "NC");
        }

        [Test]
        public void TestClassicKeyboardIncompatibleSelection()
        {
            AddStep("set classic hotkey mode", () => configManager.SetValue(OsuSetting.ModSelectHotkeyStyle, ModSelectHotkeyStyle.Classic));

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

            AddStep("press A", () => InputManager.Key(Key.A));
            AddAssert("HR panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "HR").Active.Value);

            AddStep("press A again", () => InputManager.Key(Key.A));
            AddAssert("HR panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "HR").Active.Value);

            AddStep("press D", () => InputManager.Key(Key.D));
            AddAssert("DT panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);
            AddAssert("NC panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NC").Active.Value);

            AddStep("press D again", () => InputManager.Key(Key.D));
            AddAssert("DT panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);
            AddAssert("NC panel deselected", () => !this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NC").Active.Value);

            AddStep("press Shift-D", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.D);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            AddAssert("DT panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "DT").Active.Value);
            AddAssert("NC panel selected", () => this.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.Acronym == "NC").Active.Value);

            AddStep("press J", () => InputManager.Key(Key.J));
            AddAssert("no change", () => this.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);

            AddStep("filter everything but NC", () => setFilter(mod => mod.Acronym == "NC"));

            AddStep("press A", () => InputManager.Key(Key.A));
            AddAssert("no change", () => this.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
        }

        [Test]
        public void TestApplySearchTerms()
        {
            Mod hidden = getExampleModsFor(ModType.DifficultyIncrease).Where(modState => modState.Mod is ModHidden).Select(modState => modState.Mod).Single();

            ModColumn column = null!;
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = column = new ModColumn(ModType.DifficultyIncrease, false)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AvailableMods = getExampleModsFor(ModType.DifficultyIncrease)
                }
            });

            applySearchAndAssert(hidden.Name);

            clearSearch();

            applySearchAndAssert(hidden.Acronym);

            clearSearch();

            applySearchAndAssert(hidden.Description.ToString());

            void applySearchAndAssert(string searchTerm)
            {
                AddStep("search by mod name", () => column.SearchTerm = searchTerm);

                AddAssert("only hidden is visible", () => column.ChildrenOfType<ModPanel>().Where(panel => panel.Visible).All(panel => panel.Mod is ModHidden));
            }

            void clearSearch()
            {
                AddStep("clear search", () => column.SearchTerm = string.Empty);

                AddAssert("all mods are visible", () => column.ChildrenOfType<ModPanel>().All(panel => panel.Visible));
            }
        }

        private void setFilter(Func<Mod, bool>? filter)
        {
            foreach (var modState in this.ChildrenOfType<ModColumn>().Single().AvailableMods)
                modState.ValidForSelection.Value = filter?.Invoke(modState.Mod) != false;
        }

        private partial class TestModColumn : ModColumn
        {
            public new bool SelectionAnimationRunning => base.SelectionAnimationRunning;

            public TestModColumn(ModType modType, bool allowIncompatibleSelection)
                : base(modType, allowIncompatibleSelection)
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
