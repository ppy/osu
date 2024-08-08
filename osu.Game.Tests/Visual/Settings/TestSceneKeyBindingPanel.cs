// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Rulesets.Taiko;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public partial class TestSceneKeyBindingPanel : OsuManualInputManagerTestScene
    {
        private readonly KeyBindingPanel panel;

        public TestSceneKeyBindingPanel()
        {
            Child = panel = new KeyBindingPanel();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            panel.Show();
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddUntilStep("wait for load", () => panel.ChildrenOfType<GlobalKeyBindingsSection>().Any());
            AddStep("Scroll to top", () => panel.ChildrenOfType<SettingsPanel.SettingsSectionsContainer>().First().ScrollToTop());
            AddWaitStep("wait for scroll", 5);
        }

        [Test]
        public void TestBindingTwoNonModifiers()
        {
            AddStep("press j", () => InputManager.PressKey(Key.J));
            scrollToAndStartBinding("Increase volume");
            AddStep("press k", () => InputManager.Key(Key.K));
            AddStep("release j", () => InputManager.ReleaseKey(Key.J));
            checkBinding("Increase volume", "K");
        }

        [Test]
        public void TestBindingSingleKey()
        {
            scrollToAndStartBinding("Increase volume");
            AddStep("press k", () => InputManager.Key(Key.K));
            checkBinding("Increase volume", "K");
        }

        [Test]
        public void TestBindingSingleModifier()
        {
            scrollToAndStartBinding("Increase volume");
            AddStep("press shift", () => InputManager.PressKey(Key.ShiftLeft));
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));
            checkBinding("Increase volume", "LShift");
        }

        [Test]
        public void TestBindingSingleKeyWithModifier()
        {
            scrollToAndStartBinding("Increase volume");
            AddStep("press shift", () => InputManager.PressKey(Key.ShiftLeft));
            AddStep("press k", () => InputManager.Key(Key.K));
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));
            checkBinding("Increase volume", "LShift-K");
        }

        [Test]
        public void TestBindingMouseWheelToNonGameplay()
        {
            scrollToAndStartBinding("Increase volume");
            AddStep("press k", () => InputManager.Key(Key.K));
            checkBinding("Increase volume", "K");

            AddStep("click again", () => InputManager.Click(MouseButton.Left));
            AddStep("scroll mouse wheel", () => InputManager.ScrollVerticalBy(1));

            checkBinding("Increase volume", "Wheel Up");
        }

        [Test]
        public void TestBindingMouseWheelToGameplay()
        {
            scrollToAndStartBinding("Left button");
            AddStep("press k", () => InputManager.Key(Key.Z));
            checkBinding("Left button", "Z");

            AddStep("click again", () => InputManager.Click(MouseButton.Left));
            AddStep("scroll mouse wheel", () => InputManager.ScrollVerticalBy(1));

            checkBinding("Left button", "Z");
        }

        [Test]
        public void TestClickTwiceOnClearButton()
        {
            KeyBindingRow firstRow = null;

            AddStep("click first row", () =>
            {
                firstRow = panel.ChildrenOfType<KeyBindingRow>().First();

                InputManager.MoveMouseTo(firstRow);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("schedule button clicks", () =>
            {
                var clearButton = firstRow.ChildrenOfType<KeyBindingRow.ClearButton>().Single();

                InputManager.MoveMouseTo(clearButton);

                int buttonClicks = 0;
                ScheduledDelegate clickDelegate = null;

                clickDelegate = Scheduler.AddDelayed(() =>
                {
                    InputManager.Click(MouseButton.Left);

                    if (++buttonClicks == 2)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        Debug.Assert(clickDelegate != null);
                        // ReSharper disable once AccessToModifiedClosure
                        clickDelegate.Cancel();
                    }
                }, 0, true);
            });
        }

        [Test]
        public void TestClearButtonOnBindings()
        {
            KeyBindingRow multiBindingRow = null;

            AddStep("click first row with two bindings", () =>
            {
                multiBindingRow = panel.ChildrenOfType<KeyBindingRow>().First(row => row.Defaults.Count() > 1);
                InputManager.MoveMouseTo(multiBindingRow.ChildrenOfType<OsuSpriteText>().First());
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("first binding cleared",
                () => multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().First().Text.Text,
                () => Is.EqualTo(InputSettingsStrings.ActionHasNoKeyBinding));

            AddStep("click second binding", () =>
            {
                var target = multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("second binding cleared",
                () => multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(1).Text.Text,
                () => Is.EqualTo(InputSettingsStrings.ActionHasNoKeyBinding));

            void clickClearButton()
            {
                AddStep("click clear button", () =>
                {
                    var clearButton = multiBindingRow.ChildrenOfType<KeyBindingRow.ClearButton>().Single();

                    InputManager.MoveMouseTo(clearButton);
                    InputManager.Click(MouseButton.Left);
                });
            }
        }

        [Test]
        public void TestSingleBindingResetButton()
        {
            KeyBindingRow settingsKeyBindingRow = null;

            AddStep("click first row", () =>
            {
                settingsKeyBindingRow = panel.ChildrenOfType<KeyBindingRow>().First();

                InputManager.MoveMouseTo(settingsKeyBindingRow);
                InputManager.Click(MouseButton.Left);
                InputManager.PressKey(Key.P);
                InputManager.ReleaseKey(Key.P);
            });

            AddUntilStep("restore button shown", () => settingsKeyBindingRow.ChildrenOfType<RevertToDefaultButton<bool>>().First().Alpha > 0);

            AddStep("click reset button for bindings", () =>
            {
                var resetButton = settingsKeyBindingRow.ChildrenOfType<RevertToDefaultButton<bool>>().First();

                resetButton.TriggerClick();
            });

            AddUntilStep("restore button hidden", () => settingsKeyBindingRow.ChildrenOfType<RevertToDefaultButton<bool>>().First().Alpha == 0);

            AddAssert("binding cleared",
                () => settingsKeyBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(0).KeyBinding.Value.KeyCombination.Equals(settingsKeyBindingRow.Defaults.ElementAt(0)));
        }

        [Test]
        public void TestResetAllBindingsButton()
        {
            KeyBindingRow settingsKeyBindingRow = null;

            AddStep("click first row", () =>
            {
                settingsKeyBindingRow = panel.ChildrenOfType<KeyBindingRow>().First();

                InputManager.MoveMouseTo(settingsKeyBindingRow);
                InputManager.Click(MouseButton.Left);
                InputManager.PressKey(Key.P);
                InputManager.ReleaseKey(Key.P);
            });

            AddUntilStep("restore button shown", () => settingsKeyBindingRow.ChildrenOfType<RevertToDefaultButton<bool>>().First().Alpha > 0);

            AddStep("click reset button for bindings", () =>
            {
                var resetButton = panel.ChildrenOfType<ResetButton>().First();

                resetButton.TriggerClick();
            });

            AddUntilStep("restore button hidden", () => settingsKeyBindingRow.ChildrenOfType<RevertToDefaultButton<bool>>().First().Alpha == 0);

            AddAssert("binding cleared",
                () => settingsKeyBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(0).KeyBinding.Value.KeyCombination.Equals(settingsKeyBindingRow.Defaults.ElementAt(0)));
        }

        [Test]
        public void TestClickRowSelectsFirstBinding()
        {
            KeyBindingRow multiBindingRow = null;

            AddStep("click first row with two bindings", () =>
            {
                multiBindingRow = panel.ChildrenOfType<KeyBindingRow>().First(row => row.Defaults.Count() > 1);
                InputManager.MoveMouseTo(multiBindingRow.ChildrenOfType<OsuSpriteText>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first binding selected", () => multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().First().IsBinding);

            AddStep("click second binding", () =>
            {
                var target = multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("click back binding row", () =>
            {
                multiBindingRow = panel.ChildrenOfType<KeyBindingRow>().ElementAt(10);
                InputManager.MoveMouseTo(multiBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first binding selected", () => multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().First().IsBinding);
        }

        [Test]
        public void TestFilteringHidesResetSectionButtons()
        {
            SearchTextBox searchTextBox = null;

            AddStep("add any search term", () =>
            {
                searchTextBox = panel.ChildrenOfType<SearchTextBox>().Single();
                searchTextBox.Current.Value = "chat";
            });
            AddUntilStep("all reset section bindings buttons hidden", () => panel.ChildrenOfType<ResetButton>().All(button => button.Alpha == 0));

            AddStep("clear search term", () => searchTextBox.Current.Value = string.Empty);
            AddUntilStep("all reset section bindings buttons shown", () => panel.ChildrenOfType<ResetButton>().All(button => button.Alpha == 1));
        }

        [Test]
        public void TestBindingConflictResolvedByRollbackViaMouse()
        {
            AddStep("reset taiko section to default", () =>
            {
                var section = panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset));
                section.ChildrenOfType<ResetButton>().Single().TriggerClick();
            });
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre));
            scrollToAndStartBinding("Left (rim)");
            AddStep("attempt to bind M1 to two keys", () => InputManager.Click(MouseButton.Left));

            KeyBindingConflictPopover popover = null;
            AddUntilStep("wait for popover", () => popover = panel.ChildrenOfType<KeyBindingConflictPopover>().SingleOrDefault(), () => Is.Not.Null);
            AddStep("click first button", () => popover.ChildrenOfType<RoundedButton>().First().TriggerClick());
            checkBinding("Left (centre)", "M1");
            checkBinding("Left (rim)", "M2");
        }

        [Test]
        public void TestBindingConflictResolvedByOverwriteViaMouse()
        {
            AddStep("reset taiko section to default", () =>
            {
                var section = panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset));
                section.ChildrenOfType<ResetButton>().Single().TriggerClick();
            });
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre));
            scrollToAndStartBinding("Left (rim)");
            AddStep("attempt to bind M1 to two keys", () => InputManager.Click(MouseButton.Left));

            KeyBindingConflictPopover popover = null;
            AddUntilStep("wait for popover", () => popover = panel.ChildrenOfType<KeyBindingConflictPopover>().SingleOrDefault(), () => Is.Not.Null);
            AddStep("click second button", () => popover.ChildrenOfType<RoundedButton>().ElementAt(1).TriggerClick());
            checkBinding("Left (centre)", InputSettingsStrings.ActionHasNoKeyBinding.ToString());
            checkBinding("Left (rim)", "M1");
        }

        [Test]
        public void TestBindingConflictResolvedByRollbackViaKeyboard()
        {
            AddStep("reset taiko & global sections to default", () =>
            {
                panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset))
                     .ChildrenOfType<ResetButton>().Single().TriggerClick();

                panel.ChildrenOfType<ResetButton>().First().TriggerClick();
            });
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre));
            scrollToAndStartBinding("Left (rim)");
            AddStep("attempt to bind M1 to two keys", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("wait for popover", () => panel.ChildrenOfType<KeyBindingConflictPopover>().SingleOrDefault(), () => Is.Not.Null);
            AddStep("press Esc", () => InputManager.Key(Key.Escape));
            checkBinding("Left (centre)", "M1");
            checkBinding("Left (rim)", "M2");
        }

        [Test]
        public void TestBindingConflictResolvedByOverwriteViaKeyboard()
        {
            AddStep("reset taiko & global sections to default", () =>
            {
                panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset))
                     .ChildrenOfType<ResetButton>().Single().TriggerClick();

                panel.ChildrenOfType<ResetButton>().First().TriggerClick();
            });
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre));
            scrollToAndStartBinding("Left (rim)");
            AddStep("attempt to bind M1 to two keys", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("wait for popover", () => panel.ChildrenOfType<KeyBindingConflictPopover>().SingleOrDefault(), () => Is.Not.Null);
            AddStep("press Enter", () => InputManager.Key(Key.Enter));
            checkBinding("Left (centre)", InputSettingsStrings.ActionHasNoKeyBinding.ToString());
            checkBinding("Left (rim)", "M1");
        }

        [Test]
        public void TestBindingConflictCausedByResetToDefaultOfSingleRow()
        {
            AddStep("reset taiko section to default", () =>
            {
                var section = panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset));
                section.ChildrenOfType<ResetButton>().Single().TriggerClick();
            });
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre));
            scrollToAndStartBinding("Left (centre)");
            AddStep("clear binding", () =>
            {
                var row = panel.ChildrenOfType<KeyBindingRow>().First(r => r.ChildrenOfType<OsuSpriteText>().Any(s => s.Text.ToString() == "Left (centre)"));
                row.ChildrenOfType<KeyBindingRow.ClearButton>().Single().TriggerClick();
            });
            scrollToAndStartBinding("Left (rim)");
            AddStep("bind M1", () => InputManager.Click(MouseButton.Left));

            AddStep("reset Left (centre) to default", () =>
            {
                var row = panel.ChildrenOfType<KeyBindingRow>().First(r => r.ChildrenOfType<OsuSpriteText>().Any(s => s.Text.ToString() == "Left (centre)"));
                row.ChildrenOfType<RevertToDefaultButton<bool>>().Single().TriggerClick();
            });

            KeyBindingConflictPopover popover = null;
            AddUntilStep("wait for popover", () => popover = panel.ChildrenOfType<KeyBindingConflictPopover>().SingleOrDefault(), () => Is.Not.Null);
            AddStep("click second button", () => popover.ChildrenOfType<RoundedButton>().ElementAt(1).TriggerClick());
            checkBinding("Left (centre)", "M1");
            checkBinding("Left (rim)", InputSettingsStrings.ActionHasNoKeyBinding.ToString());
        }

        [Test]
        public void TestResettingEntireSectionDoesNotCauseBindingConflicts()
        {
            AddStep("reset taiko section to default", () =>
            {
                var section = panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset));
                section.ChildrenOfType<ResetButton>().Single().TriggerClick();
            });
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre));
            scrollToAndStartBinding("Left (centre)");
            AddStep("clear binding", () =>
            {
                var row = panel.ChildrenOfType<KeyBindingRow>().First(r => r.ChildrenOfType<OsuSpriteText>().Any(s => s.Text.ToString() == "Left (centre)"));
                row.ChildrenOfType<KeyBindingRow.ClearButton>().Single().TriggerClick();
            });
            scrollToAndStartBinding("Left (rim)");
            AddStep("bind M1", () => InputManager.Click(MouseButton.Left));

            AddStep("reset taiko section to default", () =>
            {
                var section = panel.ChildrenOfType<VariantBindingsSubsection>().First(section => new TaikoRuleset().RulesetInfo.Equals(section.Ruleset));
                section.ChildrenOfType<ResetButton>().Single().TriggerClick();
            });
            AddWaitStep("wait a bit", 3);
            AddUntilStep("conflict popover not shown", () => panel.ChildrenOfType<KeyBindingConflictPopover>().SingleOrDefault(), () => Is.Null);
        }

        private void checkBinding(string name, string keyName)
        {
            AddAssert($"Check {name} is bound to {keyName}", () =>
            {
                var firstRow = panel.ChildrenOfType<KeyBindingRow>().First(r => r.ChildrenOfType<OsuSpriteText>().Any(s => s.Text.ToString() == name));
                var firstButton = firstRow.ChildrenOfType<KeyBindingRow.KeyButton>().First();

                return firstButton.Text.Text.ToString();
            }, () => Is.EqualTo(keyName));
        }

        private void scrollToAndStartBinding(string name)
        {
            KeyBindingRow.KeyButton firstButton = null;

            AddStep($"Scroll to {name}", () =>
            {
                var firstRow = panel.ChildrenOfType<KeyBindingRow>().First(r => r.ChildrenOfType<OsuSpriteText>().Any(s => s.Text.ToString() == name));
                firstButton = firstRow.ChildrenOfType<KeyBindingRow.KeyButton>().First();

                panel.ChildrenOfType<SettingsPanel.SettingsSectionsContainer>().First().ScrollTo(firstButton);
            });

            AddWaitStep("wait for scroll", 5);

            AddStep("click to bind", () =>
            {
                InputManager.MoveMouseTo(firstButton);
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
