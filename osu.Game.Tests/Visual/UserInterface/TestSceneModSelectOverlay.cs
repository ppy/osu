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
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [Description("mod select and icon display")]
    public class TestSceneModSelectOverlay : OsuTestScene
    {
        private RulesetStore rulesets;
        private ModDisplay modDisplay;
        private TestModSelectOverlay modSelect;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SelectedMods.Value = Array.Empty<Mod>();
            createDisplay(() => new TestModSelectOverlay());
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show", () => modSelect.Show());
        }

        /// <summary>
        /// Ensure that two mod overlays are not cross polluting via central settings instances.
        /// </summary>
        [Test]
        public void TestSettingsNotCrossPolluting()
        {
            Bindable<IReadOnlyList<Mod>> selectedMods2 = null;

            AddStep("select diff adjust", () => SelectedMods.Value = new Mod[] { new OsuModDifficultyAdjust() });

            AddStep("set setting", () => modSelect.ChildrenOfType<SettingsSlider<float>>().First().Current.Value = 8);

            AddAssert("ensure setting is propagated", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);

            AddStep("create second bindable", () => selectedMods2 = new Bindable<IReadOnlyList<Mod>>(new Mod[] { new OsuModDifficultyAdjust() }));

            AddStep("create second overlay", () =>
            {
                Add(modSelect = new TestModSelectOverlay().With(d =>
                {
                    d.Origin = Anchor.TopCentre;
                    d.Anchor = Anchor.TopCentre;
                    d.SelectedMods.BindTarget = selectedMods2;
                }));
            });

            AddStep("show", () => modSelect.Show());

            AddAssert("ensure first is unchanged", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);
            AddAssert("ensure second is default", () => selectedMods2.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == null);
        }

        [Test]
        public void TestSettingsResetOnDeselection()
        {
            var osuModDoubleTime = new OsuModDoubleTime { SpeedChange = { Value = 1.2 } };

            changeRuleset(0);

            AddStep("set dt mod with custom rate", () => { SelectedMods.Value = new[] { osuModDoubleTime }; });

            AddAssert("selected mod matches", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.Value == 1.2);

            AddStep("deselect", () => modSelect.DeselectAllButton.TriggerClick());
            AddAssert("selected mods empty", () => SelectedMods.Value.Count == 0);

            AddStep("reselect", () => modSelect.GetModButton(osuModDoubleTime).TriggerClick());
            AddAssert("selected mod has default value", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.IsDefault == true);
        }

        [Test]
        public void TestAnimationFlushOnClose()
        {
            changeRuleset(0);

            AddStep("Select all fun mods", () =>
            {
                modSelect.ModSectionsContainer
                         .Single(c => c.ModType == ModType.DifficultyIncrease)
                         .SelectAll();
            });

            AddUntilStep("many mods selected", () => modDisplay.Current.Value.Count >= 5);

            AddStep("trigger deselect and close overlay", () =>
            {
                modSelect.ModSectionsContainer
                         .Single(c => c.ModType == ModType.DifficultyIncrease)
                         .DeselectAll();

                modSelect.Hide();
            });

            AddAssert("all mods deselected", () => modDisplay.Current.Value.Count == 0);
        }

        [Test]
        public void TestOsuMods()
        {
            changeRuleset(0);

            var osu = new OsuRuleset();

            var easierMods = osu.GetModsFor(ModType.DifficultyReduction);
            var harderMods = osu.GetModsFor(ModType.DifficultyIncrease);

            var noFailMod = osu.GetModsFor(ModType.DifficultyReduction).FirstOrDefault(m => m is OsuModNoFail);

            var doubleTimeMod = harderMods.OfType<MultiMod>().FirstOrDefault(m => m.Mods.Any(a => a is OsuModDoubleTime));

            var easy = easierMods.FirstOrDefault(m => m is OsuModEasy);
            var hardRock = harderMods.FirstOrDefault(m => m is OsuModHardRock);

            testSingleMod(noFailMod);
            testMultiMod(doubleTimeMod);
            testIncompatibleMods(easy, hardRock);
            testDeselectAll(easierMods.Where(m => !(m is MultiMod)));
        }

        [Test]
        public void TestManiaMods()
        {
            changeRuleset(3);

            var mania = new ManiaRuleset();

            testModsWithSameBaseType(
                mania.GetAllMods().Single(m => m.GetType() == typeof(ManiaModFadeIn)),
                mania.GetAllMods().Single(m => m.GetType() == typeof(ManiaModHidden)));
        }

        [Test]
        public void TestRulesetChanges()
        {
            changeRuleset(0);

            var noFailMod = new OsuRuleset().GetModsFor(ModType.DifficultyReduction).FirstOrDefault(m => m is OsuModNoFail);

            AddStep("set mods externally", () => { SelectedMods.Value = new[] { noFailMod }; });

            changeRuleset(0);

            AddAssert("ensure mods still selected", () => modDisplay.Current.Value.SingleOrDefault(m => m is OsuModNoFail) != null);

            changeRuleset(3);

            AddAssert("ensure mods not selected", () => modDisplay.Current.Value.Count == 0);

            changeRuleset(0);

            AddAssert("ensure mods not selected", () => modDisplay.Current.Value.Count == 0);
        }

        [Test]
        public void TestExternallySetCustomizedMod()
        {
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });

            AddAssert("ensure button is selected and customized accordingly", () =>
            {
                var button = modSelect.GetModButton(SelectedMods.Value.Single());
                return ((OsuModDoubleTime)button.SelectedMod).SpeedChange.Value == 1.01;
            });
        }

        [Test]
        public void TestSettingsAreRetainedOnReload()
        {
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });

            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);

            AddStep("create overlay", () => createDisplay(() => new TestNonStackedModSelectOverlay()));

            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);
        }

        [Test]
        public void TestExternallySetModIsReplacedByOverlayInstance()
        {
            Mod external = new OsuModDoubleTime();
            Mod overlayButtonMod = null;

            changeRuleset(0);

            AddStep("set mod externally", () => { SelectedMods.Value = new[] { external }; });

            AddAssert("ensure button is selected", () =>
            {
                var button = modSelect.GetModButton(SelectedMods.Value.Single());
                overlayButtonMod = button.SelectedMod;
                return overlayButtonMod.GetType() == external.GetType();
            });

            // Right now, when an external change occurs, the ModSelectOverlay will replace the global instance with its own
            AddAssert("mod instance doesn't match", () => external != overlayButtonMod);

            AddAssert("one mod present in global selected", () => SelectedMods.Value.Count == 1);
            AddAssert("globally selected matches button's mod instance", () => SelectedMods.Value.Contains(overlayButtonMod));
            AddAssert("globally selected doesn't contain original external change", () => !SelectedMods.Value.Contains(external));
        }

        [Test]
        public void TestNonStacked()
        {
            changeRuleset(0);

            AddStep("create overlay", () => createDisplay(() => new TestNonStackedModSelectOverlay()));

            AddStep("show", () => modSelect.Show());

            AddAssert("ensure all buttons are spread out", () => modSelect.ChildrenOfType<ModButton>().All(m => m.Mods.Length <= 1));
        }

        [Test]
        public void TestChangeIsValidChangesButtonVisibility()
        {
            changeRuleset(0);

            AddAssert("double time visible", () => modSelect.ChildrenOfType<ModButton>().Any(b => b.Mods.Any(m => m is OsuModDoubleTime)));

            AddStep("make double time invalid", () => modSelect.IsValidMod = m => !(m is OsuModDoubleTime));
            AddUntilStep("double time not visible", () => modSelect.ChildrenOfType<ModButton>().All(b => !b.Mods.Any(m => m is OsuModDoubleTime)));
            AddAssert("nightcore still visible", () => modSelect.ChildrenOfType<ModButton>().Any(b => b.Mods.Any(m => m is OsuModNightcore)));

            AddStep("make double time valid again", () => modSelect.IsValidMod = m => true);
            AddUntilStep("double time visible", () => modSelect.ChildrenOfType<ModButton>().Any(b => b.Mods.Any(m => m is OsuModDoubleTime)));
            AddAssert("nightcore still visible", () => modSelect.ChildrenOfType<ModButton>().Any(b => b.Mods.Any(m => m is OsuModNightcore)));
        }

        [Test]
        public void TestChangeIsValidPreservesSelection()
        {
            changeRuleset(0);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => modSelect.ChildrenOfType<ModButton>().Count(b => b.Selected) == 2);

            AddStep("make NF invalid", () => modSelect.IsValidMod = m => !(m is ModNoFail));
            AddAssert("DT + HD still selected", () => modSelect.ChildrenOfType<ModButton>().Count(b => b.Selected) == 2);
        }

        [Test]
        public void TestUnimplementedModIsUnselectable()
        {
            var testRuleset = new TestUnimplementedModOsuRuleset();
            changeTestRuleset(testRuleset.RulesetInfo);

            var conversionMods = testRuleset.GetModsFor(ModType.Conversion);

            var unimplementedMod = conversionMods.FirstOrDefault(m => m is TestUnimplementedMod);

            testUnimplementedMod(unimplementedMod);
        }

        private void testSingleMod(Mod mod)
        {
            selectNext(mod);
            checkSelected(mod);

            selectPrevious(mod);
            checkNotSelected(mod);

            selectNext(mod);
            selectNext(mod);
            checkNotSelected(mod);

            selectPrevious(mod);
            selectPrevious(mod);
            checkNotSelected(mod);
        }

        private void testMultiMod(MultiMod multiMod)
        {
            foreach (var mod in multiMod.Mods)
            {
                selectNext(mod);
                checkSelected(mod);
            }

            for (int index = multiMod.Mods.Length - 1; index >= 0; index--)
                selectPrevious(multiMod.Mods[index]);

            foreach (var mod in multiMod.Mods)
                checkNotSelected(mod);
        }

        private void testUnimplementedMod(Mod mod)
        {
            selectNext(mod);
            checkNotSelected(mod);
        }

        private void testIncompatibleMods(Mod modA, Mod modB)
        {
            selectNext(modA);
            checkSelected(modA);
            checkNotSelected(modB);

            selectNext(modB);
            checkSelected(modB);
            checkNotSelected(modA);

            selectPrevious(modB);
            checkNotSelected(modA);
            checkNotSelected(modB);
        }

        private void testDeselectAll(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
                selectNext(mod);

            AddAssert("check for any selection", () => modSelect.SelectedMods.Value.Any());
            AddStep("deselect all", () => modSelect.DeselectAllButton.Action.Invoke());
            AddAssert("check for no selection", () => !modSelect.SelectedMods.Value.Any());
        }

        private void testModsWithSameBaseType(Mod modA, Mod modB)
        {
            selectNext(modA);
            checkSelected(modA);
            selectNext(modB);
            checkSelected(modB);

            // Backwards
            selectPrevious(modA);
            checkSelected(modA);
        }

        private void selectNext(Mod mod) => AddStep($"left click {mod.Name}", () => modSelect.GetModButton(mod)?.SelectNext(1));

        private void selectPrevious(Mod mod) => AddStep($"right click {mod.Name}", () => modSelect.GetModButton(mod)?.SelectNext(-1));

        private void checkSelected(Mod mod)
        {
            AddAssert($"check {mod.Name} is selected", () =>
            {
                var button = modSelect.GetModButton(mod);
                return modSelect.SelectedMods.Value.SingleOrDefault(m => m.Name == mod.Name) != null && button.SelectedMod.GetType() == mod.GetType() && button.Selected;
            });
        }

        private void changeRuleset(int? id)
        {
            AddStep($"change ruleset to {(id?.ToString() ?? "none")}", () => { Ruleset.Value = rulesets.AvailableRulesets.FirstOrDefault(r => r.ID == id); });
            waitForLoad();
        }

        private void changeTestRuleset(RulesetInfo rulesetInfo)
        {
            AddStep($"change ruleset to {rulesetInfo.Name}", () => { Ruleset.Value = rulesetInfo; });
            waitForLoad();
        }

        private void waitForLoad() =>
            AddUntilStep("wait for icons to load", () => modSelect.AllLoaded);

        private void checkNotSelected(Mod mod)
        {
            AddAssert($"check {mod.Name} is not selected", () =>
            {
                var button = modSelect.GetModButton(mod);
                return modSelect.SelectedMods.Value.All(m => m.GetType() != mod.GetType()) && button.SelectedMod?.GetType() != mod.GetType();
            });
        }

        private void createDisplay(Func<TestModSelectOverlay> createOverlayFunc)
        {
            Children = new Drawable[]
            {
                modSelect = createOverlayFunc().With(d =>
                {
                    d.Origin = Anchor.BottomCentre;
                    d.Anchor = Anchor.BottomCentre;
                    d.SelectedMods.BindTarget = SelectedMods;
                }),
                modDisplay = new ModDisplay
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Position = new Vector2(-5, 25),
                    Current = { BindTarget = modSelect.SelectedMods }
                }
            };
        }

        private class TestModSelectOverlay : UserModSelectOverlay
        {
            public new Bindable<IReadOnlyList<Mod>> SelectedMods => base.SelectedMods;

            public bool AllLoaded => ModSectionsContainer.Children.All(c => c.ModIconsLoaded);

            public new FillFlowContainer<ModSection> ModSectionsContainer =>
                base.ModSectionsContainer;

            public ModButton GetModButton(Mod mod)
            {
                var section = ModSectionsContainer.Children.Single(s => s.ModType == mod.Type);
                return section.ButtonsContainer.OfType<ModButton>().Single(b => b.Mods.Any(m => m.GetType() == mod.GetType()));
            }

            public new TriangleButton DeselectAllButton => base.DeselectAllButton;

            public new Color4 LowMultiplierColour => base.LowMultiplierColour;
            public new Color4 HighMultiplierColour => base.HighMultiplierColour;
        }

        private class TestNonStackedModSelectOverlay : TestModSelectOverlay
        {
            protected override bool Stacked => false;
        }

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
            public override IEnumerable<Mod> GetModsFor(ModType type)
            {
                if (type == ModType.Conversion) return base.GetModsFor(type).Concat(new[] { new TestUnimplementedMod() });

                return base.GetModsFor(type);
            }
        }
    }
}
