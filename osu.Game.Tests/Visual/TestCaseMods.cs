// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Screens.Play.HUD;
using OpenTK;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using System.Linq;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.UI;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [Description("mod select and icon display")]
    public class TestCaseMods : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ModSelectOverlay),
            typeof(ModDisplay),
            typeof(ModSection),
            typeof(ModIcon),
            typeof(ModButton),
            typeof(ModButtonEmpty),
            typeof(DifficultyReductionSection),
            typeof(DifficultyIncreaseSection),
            typeof(SpecialSection),
        };

        private RulesetStore rulesets;
        private ModDisplay modDisplay;
        private TestModSelectOverlay modSelect;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(modSelect = new TestModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            Add(modDisplay = new ModDisplay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Position = new Vector2(0, 25),
            });

            modDisplay.Current.BindTo(modSelect.SelectedMods);

            AddStep("Toggle", modSelect.ToggleVisibility);
            AddStep("Hide", modSelect.Hide);
            AddStep("Show", modSelect.Show);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                Ruleset ruleset = rulesetInfo.CreateInstance();
                AddStep($"switch to {ruleset.Description}", () => Ruleset.Value = rulesetInfo);

                switch (ruleset)
                {
                    case OsuRuleset or:
                        testOsuMods(or);
                        break;
                    case ManiaRuleset mr:
                        testManiaMods(mr);
                        break;
                }
            }
        }

        private void testOsuMods(OsuRuleset ruleset)
        {
            var easierMods = ruleset.GetModsFor(ModType.DifficultyReduction);
            var harderMods = ruleset.GetModsFor(ModType.DifficultyIncrease);
            var assistMods = ruleset.GetModsFor(ModType.Special);

            var noFailMod = easierMods.FirstOrDefault(m => m is OsuModNoFail);
            var hiddenMod = harderMods.FirstOrDefault(m => m is OsuModHidden);

            var doubleTimeMod = harderMods.OfType<MultiMod>().FirstOrDefault(m => m.Mods.Any(a => a is OsuModDoubleTime));

            var autoPilotMod = assistMods.FirstOrDefault(m => m is OsuModAutopilot);

            var easy = easierMods.FirstOrDefault(m => m is OsuModEasy);
            var hardRock = harderMods.FirstOrDefault(m => m is OsuModHardRock);

            testSingleMod(noFailMod);
            testMultiMod(doubleTimeMod);
            testIncompatibleMods(easy, hardRock);
            testDeselectAll(easierMods.Where(m => !(m is MultiMod)));
            testMultiplierTextColour(noFailMod, modSelect.LowMultiplierColour);
            testMultiplierTextColour(hiddenMod, modSelect.HighMultiplierColour);

            testUnimplmentedMod(autoPilotMod);
        }

        private void testManiaMods(ManiaRuleset ruleset)
        {
            testRankedText(ruleset.GetModsFor(ModType.Special).First(m => m is ManiaModRandom));
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

        private void testUnimplmentedMod(Mod mod)
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
            AddStep("deselect all", modSelect.DeselectAllButton.Action.Invoke);
            AddAssert("check for no selection", () => !modSelect.SelectedMods.Value.Any());
        }

        private void testMultiplierTextColour(Mod mod, Color4 colour)
        {
            checkLabelColor(Color4.White);
            selectNext(mod);
            AddWaitStep(1, "wait for changing colour");
            checkLabelColor(colour);
            selectPrevious(mod);
            AddWaitStep(1, "wait for changing colour");
            checkLabelColor(Color4.White);
        }

        private void testRankedText(Mod mod)
        {
            AddWaitStep(1, "wait for fade");
            AddAssert("check for ranked", () => modSelect.UnrankedLabel.Alpha == 0);
            selectNext(mod);
            AddWaitStep(1, "wait for fade");
            AddAssert("check for unranked", () => modSelect.UnrankedLabel.Alpha != 0);
            selectPrevious(mod);
            AddWaitStep(1, "wait for fade");
            AddAssert("check for ranked", () => modSelect.UnrankedLabel.Alpha == 0);
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

        private void checkNotSelected(Mod mod)
        {
            AddAssert($"check {mod.Name} is not selected", () =>
            {
                var button = modSelect.GetModButton(mod);
                return modSelect.SelectedMods.Value.All(m => m.GetType() != mod.GetType()) && button.SelectedMod?.GetType() != mod.GetType();
            });
        }

        private void checkLabelColor(Color4 color) => AddAssert("check label has expected colour", () => modSelect.MultiplierLabel.Colour.AverageColour == color);

        private class TestModSelectOverlay : ModSelectOverlay
        {
            public ModButton GetModButton(Mod mod)
            {
                var section = ModSectionsContainer.Children.Single(s => s.ModType == mod.Type);
                return section.ButtonsContainer.OfType<ModButton>().Single(b => b.Mods.Any(m => m.GetType() == mod.GetType()));
            }

            public new OsuSpriteText MultiplierLabel => base.MultiplierLabel;
            public new OsuSpriteText UnrankedLabel => base.UnrankedLabel;
            public new TriangleButton DeselectAllButton => base.DeselectAllButton;

            public new Color4 LowMultiplierColour => base.LowMultiplierColour;
            public new Color4 HighMultiplierColour => base.HighMultiplierColour;
        }
    }
}
