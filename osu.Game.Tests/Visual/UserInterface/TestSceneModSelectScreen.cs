// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneModSelectScreen : OsuManualInputManagerTestScene
    {
        private ModSelectScreen modSelectScreen;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset mods", () => SelectedMods.SetDefault());
        }

        private void createScreen() => AddStep("create screen", () => Child = modSelectScreen = new ModSelectScreen
        {
            RelativeSizeAxes = Axes.Both,
            State = { Value = Visibility.Visible },
            SelectedMods = { BindTarget = SelectedMods }
        });

        [Test]
        public void TestStateChange()
        {
            createScreen();
            AddToggleStep("toggle state", visible => modSelectScreen.State.Value = visible ? Visibility.Visible : Visibility.Hidden);
        }

        [Test]
        public void TestPreexistingSelection()
        {
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            createScreen();
            AddUntilStep("two panels active", () => modSelectScreen.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (multiplier, mod) => multiplier * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, modSelectScreen.ChildrenOfType<DifficultyMultiplierDisplay>().Single().Current.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
        }

        [Test]
        public void TestExternalSelection()
        {
            createScreen();
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            AddUntilStep("two panels active", () => modSelectScreen.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (multiplier, mod) => multiplier * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, modSelectScreen.ChildrenOfType<DifficultyMultiplierDisplay>().Single().Current.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
        }

        [Test]
        public void TestRulesetChange()
        {
            createScreen();
            AddStep("change to osu!", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("change to osu!taiko", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("change to osu!catch", () => Ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("change to osu!mania", () => Ruleset.Value = new ManiaRuleset().RulesetInfo);
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

        private void assertCustomisationToggleState(bool disabled, bool active)
        {
            ShearedToggleButton getToggle() => modSelectScreen.ChildrenOfType<ShearedToggleButton>().Single();

            AddAssert($"customisation toggle is {(disabled ? "" : "not ")}disabled", () => getToggle().Active.Disabled == disabled);
            AddAssert($"customisation toggle is {(active ? "" : "not ")}active", () => getToggle().Active.Value == active);
        }
    }
}
