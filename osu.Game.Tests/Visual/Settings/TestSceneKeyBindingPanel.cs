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
            BasicKeyBindingRow firstRow = null;

            AddStep("click first row", () =>
            {
                firstRow = panel.ChildrenOfType<BasicKeyBindingRow>().First();

                InputManager.MoveMouseTo(firstRow);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("schedule button clicks", () =>
            {
                var clearButton = firstRow.ChildrenOfType<BasicKeyBindingRow.ClearButton>().Single();

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
            BasicKeyBindingRow multiBindingRow = null;

            AddStep("click first row with two bindings", () =>
            {
                multiBindingRow = panel.ChildrenOfType<BasicKeyBindingRow>().First(row => row.Defaults.Count() > 1);
                InputManager.MoveMouseTo(multiBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("first binding cleared", () => string.IsNullOrEmpty(multiBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().First().Text.Text.ToString()));

            AddStep("click second binding", () =>
            {
                var target = multiBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("second binding cleared", () => string.IsNullOrEmpty(multiBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().ElementAt(1).Text.Text.ToString()));

            void clickClearButton()
            {
                AddStep("click clear button", () =>
                {
                    var clearButton = multiBindingRow.ChildrenOfType<BasicKeyBindingRow.ClearButton>().Single();

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

            AddUntilStep("restore button shown", () => settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First().Alpha > 0);

            AddStep("click reset button for bindings", () =>
            {
                var resetButton = settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First();

                resetButton.Click();
            });

            AddUntilStep("restore button hidden", () => settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First().Alpha == 0);

            AddAssert("binding cleared", () => settingsKeyBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().ElementAt(0).KeyBinding.KeyCombination.Equals(settingsKeyBindingRow.BasicKeyBindingRow.Defaults.ElementAt(0)));
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

            AddUntilStep("restore button shown", () => settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First().Alpha > 0);

            AddStep("click reset button for bindings", () =>
            {
                var resetButton = panel.ChildrenOfType<ResetButton>().First();

                resetButton.Click();
            });

            AddUntilStep("restore button hidden", () => settingsKeyBindingRow.ChildrenOfType<RestoreDefaultValueButton<bool>>().First().Alpha == 0);

            AddAssert("binding cleared", () => settingsKeyBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().ElementAt(0).KeyBinding.KeyCombination.Equals(settingsKeyBindingRow.BasicKeyBindingRow.Defaults.ElementAt(0)));
        }

        [Test]
        public void TestClickRowSelectsFirstBinding()
        {
            BasicKeyBindingRow multiBindingRow = null;

            AddStep("click first row with two bindings", () =>
            {
                multiBindingRow = panel.ChildrenOfType<BasicKeyBindingRow>().First(row => row.Defaults.Count() > 1);
                InputManager.MoveMouseTo(multiBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first binding selected", () => multiBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().First().IsBinding);

            AddStep("click second binding", () =>
            {
                var target = multiBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("click back binding row", () =>
            {
                multiBindingRow = panel.ChildrenOfType<BasicKeyBindingRow>().ElementAt(10);
                InputManager.MoveMouseTo(multiBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first binding selected", () => multiBindingRow.ChildrenOfType<BasicKeyBindingRow.KeyButton>().First().IsBinding);
        }
    }
}