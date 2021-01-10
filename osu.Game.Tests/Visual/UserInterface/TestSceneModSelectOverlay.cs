// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
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
            Children = new Drawable[]
            {
                modSelect = new TestModSelectOverlay
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    SelectedMods = { BindTarget = SelectedMods }
                },

                modDisplay = new ModDisplay
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Position = new Vector2(-5, 25),
                    Current = { BindTarget = modSelect.SelectedMods }
                }
            };
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show", () => modSelect.Show());
        }

        [Test]
        public void TestOsuMods()
        {
            changeRuleset(0);

            var osu = new OsuRuleset();

            var easierMods = osu.GetModsFor(ModType.DifficultyReduction);
            var harderMods = osu.GetModsFor(ModType.DifficultyIncrease);
            var conversionMods = osu.GetModsFor(ModType.Conversion);

            var noFailMod = osu.GetModsFor(ModType.DifficultyReduction).FirstOrDefault(m => m is OsuModNoFail);
            var hiddenMod = harderMods.FirstOrDefault(m => m is OsuModHidden);

            var doubleTimeMod = harderMods.OfType<MultiMod>().FirstOrDefault(m => m.Mods.Any(a => a is OsuModDoubleTime));

            var targetMod = conversionMods.FirstOrDefault(m => m is OsuModTarget);

            var easy = easierMods.FirstOrDefault(m => m is OsuModEasy);
            var hardRock = harderMods.FirstOrDefault(m => m is OsuModHardRock);

            testSingleMod(noFailMod);
            testMultiMod(doubleTimeMod);
            testIncompatibleMods(easy, hardRock);
            testDeselectAll(easierMods.Where(m => !(m is MultiMod)));
            testMultiplierTextColour(noFailMod, () => modSelect.LowMultiplierColour);
            testMultiplierTextColour(hiddenMod, () => modSelect.HighMultiplierColour);

            testUnimplementedMod(targetMod);
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

            AddAssert("ensure mods still selected", () => modDisplay.Current.Value.Single(m => m is OsuModNoFail) != null);

            changeRuleset(3);

            AddAssert("ensure mods not selected", () => modDisplay.Current.Value.Count == 0);

            changeRuleset(0);

            AddAssert("ensure mods not selected", () => modDisplay.Current.Value.Count == 0);
        }

        [Test]
        public void TestExternallySetCustomizedMod()
        {
            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });

            AddAssert("ensure button is selected and customized accordingly", () =>
            {
                var button = modSelect.GetModButton(SelectedMods.Value.Single());
                return ((OsuModDoubleTime)button.SelectedMod).SpeedChange.Value == 1.01;
            });
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

        private void testMultiplierTextColour(Mod mod, Func<Color4> getCorrectColour)
        {
            checkLabelColor(() => Color4.White);
            selectNext(mod);
            AddWaitStep("wait for changing colour", 1);
            checkLabelColor(getCorrectColour);
            selectPrevious(mod);
            AddWaitStep("wait for changing colour", 1);
            checkLabelColor(() => Color4.White);
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
                return modSelect.SelectedMods.Value.Single(m => m.Name == mod.Name) != null && button.SelectedMod.GetType() == mod.GetType() && button.Selected;
            });
        }

        private void changeRuleset(int? id)
        {
            AddStep($"change ruleset to {(id?.ToString() ?? "none")}", () => { Ruleset.Value = rulesets.AvailableRulesets.FirstOrDefault(r => r.ID == id); });
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

        private void checkLabelColor(Func<Color4> getColour) => AddAssert("check label has expected colour", () => modSelect.MultiplierLabel.Colour.AverageColour == getColour());

        private class TestModSelectOverlay : ModSelectOverlay
        {
            public new Bindable<IReadOnlyList<Mod>> SelectedMods => base.SelectedMods;

            public bool AllLoaded => ModSectionsContainer.Children.All(c => c.ModIconsLoaded);

            public ModButton GetModButton(Mod mod)
            {
                var section = ModSectionsContainer.Children.Single(s => s.ModType == mod.Type);
                return section.ButtonsContainer.OfType<ModButton>().Single(b => b.Mods.Any(m => m.GetType() == mod.GetType()));
            }

            public new OsuSpriteText MultiplierLabel => base.MultiplierLabel;
            public new TriangleButton DeselectAllButton => base.DeselectAllButton;

            public new Color4 LowMultiplierColour => base.LowMultiplierColour;
            public new Color4 HighMultiplierColour => base.HighMultiplierColour;
        }
    }
}
