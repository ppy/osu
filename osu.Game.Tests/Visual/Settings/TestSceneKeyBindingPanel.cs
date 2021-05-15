// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Overlays;
using osu.Game.Overlays.KeyBinding;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneKeyBindingPanel : OsuManualInputManagerTestScene
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
                InputManager.MoveMouseTo(multiBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("first binding cleared", () => string.IsNullOrEmpty(multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().First().Text.Text.ToString()));

            AddStep("click second binding", () =>
            {
                var target = multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("second binding cleared", () => string.IsNullOrEmpty(multiBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(1).Text.Text.ToString()));

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
        public void TestSingleBindResetButton()
        {
            SettingsKeyBindingRow settingsKeyBindingRow = null;

            AddStep("click first row with two bindings", () =>
            {
                settingsKeyBindingRow = panel.ChildrenOfType<SettingsKeyBindingRow>().First(row => row.KeyBindingRow.Defaults.Count() > 1);
                InputManager.MoveMouseTo(settingsKeyBindingRow.KeyBindingRow);
                InputManager.Click(MouseButton.Left);
                InputManager.PressKey(Key.P);
                InputManager.ReleaseKey(Key.P);
            });

            AddUntilStep("restore button shown", () => settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First().Alpha > 0);

            AddStep("click reset button for bindings", () =>
            {
                var clearButton = settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().Single();

                InputManager.MoveMouseTo(clearButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("restore button hidden", () => settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First().Alpha == 0);

            AddAssert("first binding cleared", () => settingsKeyBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(0).KeyBinding.KeyCombination.Equals(settingsKeyBindingRow.KeyBindingRow.Defaults.ElementAt(0)));
            AddAssert("second binding cleared", () => settingsKeyBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(1).KeyBinding.KeyCombination.Equals(settingsKeyBindingRow.KeyBindingRow.Defaults.ElementAt(1)));
        }

        [Test]
        public void TestResetAllBindingsButton()
        {
            KeyBindingRow keyBindingRow = null;

            AddStep("click first row and press p", () =>
            {
                keyBindingRow = panel.ChildrenOfType<KeyBindingRow>().First();
                InputManager.MoveMouseTo(keyBindingRow);
                InputManager.Click(MouseButton.Left);
                InputManager.PressKey(Key.P);
                InputManager.ReleaseKey(Key.P);
            });
            AddStep("click reset button for all bindings", () =>
            {
                var clearButton = panel.ChildrenOfType<ResetButton>().First();

                InputManager.MoveMouseTo(clearButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("bindings cleared", () => keyBindingRow.ChildrenOfType<KeyBindingRow.KeyButton>().ElementAt(0).KeyBinding.KeyCombination.Equals(keyBindingRow.Defaults.ElementAt(0)));
        }

        [Test]
        public void TestClickRowSelectsFirstBinding()
        {
            KeyBindingRow multiBindingRow = null;

            AddStep("click first row with two bindings", () =>
            {
                multiBindingRow = panel.ChildrenOfType<KeyBindingRow>().First(row => row.Defaults.Count() > 1);
                InputManager.MoveMouseTo(multiBindingRow);
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
    }
}