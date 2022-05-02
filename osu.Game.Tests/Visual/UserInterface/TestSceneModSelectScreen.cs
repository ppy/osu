// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneModSelectScreen : OsuManualInputManagerTestScene
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        private UserModSelectScreen modSelectScreen;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear contents", Clear);
            AddStep("reset ruleset", () => Ruleset.Value = rulesetStore.GetRuleset(0));
            AddStep("reset mods", () => SelectedMods.SetDefault());
        }

        private void createScreen()
        {
            AddStep("create screen", () => Child = modSelectScreen = new UserModSelectScreen
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
                SelectedMods = { BindTarget = SelectedMods }
            });
            waitForColumnLoad();
        }

        [Test]
        public void TestStateChange()
        {
            createScreen();
            AddStep("toggle state", () => modSelectScreen.ToggleVisibility());
        }

        [Test]
        public void TestPreexistingSelection()
        {
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            createScreen();
            AddUntilStep("two panels active", () => modSelectScreen.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (m, mod) => m * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, modSelectScreen.ChildrenOfType<DifficultyMultiplierDisplay>().Single().Current.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
            AddAssert("setting items created", () => modSelectScreen.ChildrenOfType<ISettingsItem>().Any());
        }

        [Test]
        public void TestExternalSelection()
        {
            createScreen();
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            AddUntilStep("two panels active", () => modSelectScreen.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (m, mod) => m * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, modSelectScreen.ChildrenOfType<DifficultyMultiplierDisplay>().Single().Current.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
            AddAssert("setting items created", () => modSelectScreen.ChildrenOfType<ISettingsItem>().Any());
        }

        [Test]
        public void TestRulesetChange()
        {
            createScreen();
            changeRuleset(0);
            changeRuleset(1);
            changeRuleset(2);
            changeRuleset(3);
        }

        [Test]
        public void TestIncompatibilityToggling()
        {
            createScreen();
            changeRuleset(0);

            AddStep("activate DT", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("DT active", () => SelectedMods.Value.Single().GetType() == typeof(OsuModDoubleTime));

            AddStep("activate NC", () => getPanelForMod(typeof(OsuModNightcore)).TriggerClick());
            AddAssert("only NC active", () => SelectedMods.Value.Single().GetType() == typeof(OsuModNightcore));

            AddStep("activate HR", () => getPanelForMod(typeof(OsuModHardRock)).TriggerClick());
            AddAssert("NC+HR active", () => SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModNightcore))
                                            && SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModHardRock)));

            AddStep("activate MR", () => getPanelForMod(typeof(OsuModMirror)).TriggerClick());
            AddAssert("NC+MR active", () => SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModNightcore))
                                            && SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModMirror)));
        }

        [Test]
        public void TestDimmedState()
        {
            createScreen();
            changeRuleset(0);

            AddUntilStep("any column dimmed", () => this.ChildrenOfType<ModColumn>().Any(column => !column.Active.Value));

            ModColumn lastColumn = null;

            AddAssert("last column dimmed", () => !this.ChildrenOfType<ModColumn>().Last().Active.Value);
            AddStep("request scroll to last column", () =>
            {
                var lastDimContainer = this.ChildrenOfType<ModSelectScreen.ColumnDimContainer>().Last();
                lastColumn = lastDimContainer.Column;
                lastDimContainer.RequestScroll?.Invoke(lastDimContainer);
            });
            AddUntilStep("column undimmed", () => lastColumn.Active.Value);

            AddStep("click panel", () =>
            {
                InputManager.MoveMouseTo(lastColumn.ChildrenOfType<ModPanel>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("panel selected", () => lastColumn.ChildrenOfType<ModPanel>().First().Active.Value);
        }

        [Test]
        public void TestCustomisationToggleState()
        {
            createScreen();
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select customisable mod", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod requiring configuration", () => SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("dismiss mod customisation", () =>
            {
                InputManager.MoveMouseTo(modSelectScreen.ChildrenOfType<ShearedToggleButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("append another mod not requiring config", () => SelectedMods.Value = SelectedMods.Value.Append(new OsuModFlashlight()).ToArray());
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select mod requiring configuration", () => SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false); // config was dismissed without explicit user action.
        }

        /// <summary>
        /// Ensure that two mod overlays are not cross polluting via central settings instances.
        /// </summary>
        [Test]
        public void TestSettingsNotCrossPolluting()
        {
            Bindable<IReadOnlyList<Mod>> selectedMods2 = null;
            ModSelectScreen modSelectScreen2 = null;

            createScreen();
            AddStep("select diff adjust", () => SelectedMods.Value = new Mod[] { new OsuModDifficultyAdjust() });

            AddStep("set setting", () => modSelectScreen.ChildrenOfType<SettingsSlider<float>>().First().Current.Value = 8);

            AddAssert("ensure setting is propagated", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);

            AddStep("create second bindable", () => selectedMods2 = new Bindable<IReadOnlyList<Mod>>(new Mod[] { new OsuModDifficultyAdjust() }));

            AddStep("create second overlay", () =>
            {
                Add(modSelectScreen2 = new UserModSelectScreen().With(d =>
                {
                    d.Origin = Anchor.TopCentre;
                    d.Anchor = Anchor.TopCentre;
                    d.SelectedMods.BindTarget = selectedMods2;
                }));
            });

            AddStep("show", () => modSelectScreen2.Show());

            AddAssert("ensure first is unchanged", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);
            AddAssert("ensure second is default", () => selectedMods2.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == null);
        }

        [Test]
        public void TestSettingsResetOnDeselection()
        {
            var osuModDoubleTime = new OsuModDoubleTime { SpeedChange = { Value = 1.2 } };

            createScreen();
            changeRuleset(0);

            AddStep("set dt mod with custom rate", () => { SelectedMods.Value = new[] { osuModDoubleTime }; });

            AddAssert("selected mod matches", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.Value == 1.2);

            AddStep("deselect", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("selected mods empty", () => SelectedMods.Value.Count == 0);

            AddStep("reselect", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("selected mod has default value", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.IsDefault == true);
        }

        [Test]
        public void TestAnimationFlushOnClose()
        {
            createScreen();
            changeRuleset(0);

            AddStep("Select all fun mods", () =>
            {
                modSelectScreen.ChildrenOfType<ModColumn>()
                               .Single(c => c.ModType == ModType.DifficultyIncrease)
                               .SelectAll();
            });

            AddUntilStep("many mods selected", () => SelectedMods.Value.Count >= 5);

            AddStep("trigger deselect and close overlay", () =>
            {
                modSelectScreen.ChildrenOfType<ModColumn>()
                               .Single(c => c.ModType == ModType.DifficultyIncrease)
                               .DeselectAll();

                modSelectScreen.Hide();
            });

            AddAssert("all mods deselected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestRulesetChanges()
        {
            createScreen();
            changeRuleset(0);

            var noFailMod = new OsuRuleset().GetModsFor(ModType.DifficultyReduction).FirstOrDefault(m => m is OsuModNoFail);

            AddStep("set mods externally", () => { SelectedMods.Value = new[] { noFailMod }; });

            changeRuleset(0);

            AddAssert("ensure mods still selected", () => SelectedMods.Value.SingleOrDefault(m => m is OsuModNoFail) != null);

            changeRuleset(3);

            AddAssert("ensure mods not selected", () => SelectedMods.Value.Count == 0);

            changeRuleset(0);

            AddAssert("ensure mods not selected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestExternallySetCustomizedMod()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });

            AddAssert("ensure button is selected and customized accordingly", () =>
            {
                var button = getPanelForMod(SelectedMods.Value.Single().GetType());
                return ((OsuModDoubleTime)button.Mod).SpeedChange.Value == 1.01;
            });
        }

        [Test]
        public void TestSettingsAreRetainedOnReload()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });
            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);

            createScreen();
            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);
        }

        [Test]
        public void TestExternallySetModIsReplacedByOverlayInstance()
        {
            Mod external = new OsuModDoubleTime();
            Mod overlayButtonMod = null;

            createScreen();
            changeRuleset(0);

            AddStep("set mod externally", () => { SelectedMods.Value = new[] { external }; });

            AddAssert("ensure button is selected", () =>
            {
                var button = getPanelForMod(SelectedMods.Value.Single().GetType());
                overlayButtonMod = button.Mod;
                return button.Active.Value;
            });

            // Right now, when an external change occurs, the ModSelectOverlay will replace the global instance with its own
            AddAssert("mod instance doesn't match", () => external != overlayButtonMod);

            AddAssert("one mod present in global selected", () => SelectedMods.Value.Count == 1);
            AddAssert("globally selected matches button's mod instance", () => SelectedMods.Value.Contains(overlayButtonMod));
            AddAssert("globally selected doesn't contain original external change", () => !SelectedMods.Value.Contains(external));
        }

        [Test]
        public void TestChangeIsValidChangesButtonVisibility()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("double time visible", () => modSelectScreen.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).Any(panel => !panel.Filtered.Value));

            AddStep("make double time invalid", () => modSelectScreen.IsValidMod = m => !(m is OsuModDoubleTime));
            AddUntilStep("double time not visible", () => modSelectScreen.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).All(panel => panel.Filtered.Value));
            AddAssert("nightcore still visible", () => modSelectScreen.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModNightcore).Any(panel => !panel.Filtered.Value));

            AddStep("make double time valid again", () => modSelectScreen.IsValidMod = m => true);
            AddUntilStep("double time visible", () => modSelectScreen.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).Any(panel => !panel.Filtered.Value));
            AddAssert("nightcore still visible", () => modSelectScreen.ChildrenOfType<ModPanel>().Where(b => b.Mod is OsuModNightcore).Any(panel => !panel.Filtered.Value));
        }

        [Test]
        public void TestChangeIsValidPreservesSelection()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => modSelectScreen.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);

            AddStep("make NF invalid", () => modSelectScreen.IsValidMod = m => !(m is ModNoFail));
            AddAssert("DT + HD still selected", () => modSelectScreen.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
        }

        [Test]
        public void TestUnimplementedModIsUnselectable()
        {
            var testRuleset = new TestUnimplementedModOsuRuleset();

            createScreen();

            AddStep("set ruleset", () => Ruleset.Value = testRuleset.RulesetInfo);
            waitForColumnLoad();

            AddAssert("unimplemented mod panel is filtered", () => getPanelForMod(typeof(TestUnimplementedMod)).Filtered.Value);
        }

        private void waitForColumnLoad() => AddUntilStep("all column content loaded",
            () => modSelectScreen.ChildrenOfType<ModColumn>().Any() && modSelectScreen.ChildrenOfType<ModColumn>().All(column => column.IsLoaded && column.ItemsLoaded));

        private void changeRuleset(int id)
        {
            AddStep($"set ruleset to {id}", () => Ruleset.Value = rulesetStore.GetRuleset(id));
            waitForColumnLoad();
        }

        private void assertCustomisationToggleState(bool disabled, bool active)
        {
            ShearedToggleButton getToggle() => modSelectScreen.ChildrenOfType<ShearedToggleButton>().Single();

            AddAssert($"customisation toggle is {(disabled ? "" : "not ")}disabled", () => getToggle().Active.Disabled == disabled);
            AddAssert($"customisation toggle is {(active ? "" : "not ")}active", () => getToggle().Active.Value == active);
        }

        private ModPanel getPanelForMod(Type modType)
            => modSelectScreen.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.GetType() == modType);

        private class TestUnimplementedMod : Mod
        {
            public override string Name => "Unimplemented mod";
            public override string Acronym => "UM";
            public override string Description => "A mod that is not implemented.";
            public override double ScoreMultiplier => 1;
            public override ModType Type => ModType.Conversion;
        }

        private class TestUnimplementedModOsuRuleset : OsuRuleset
        {
            public override string ShortName => "unimplemented";

            public override IEnumerable<Mod> GetModsFor(ModType type)
            {
                if (type == ModType.Conversion) return base.GetModsFor(type).Concat(new[] { new TestUnimplementedMod() });

                return base.GetModsFor(type);
            }
        }
    }
}
