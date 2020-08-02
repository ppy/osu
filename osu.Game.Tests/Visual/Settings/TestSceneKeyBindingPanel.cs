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
                    InputManager.PressButton(MouseButton.Left);
                    InputManager.ReleaseButton(MouseButton.Left);

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
            KeyBindingRow backBindingRow = null;

            AddStep("click back binding row", () =>
            {
                backBindingRow = panel.ChildrenOfType<KeyBindingRow>().ElementAt(10);
                InputManager.MoveMouseTo(backBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("first binding cleared", () => string.IsNullOrEmpty(backBindingRow.Buttons.First().Text.Text));

            AddStep("click second binding", () =>
            {
                var target = backBindingRow.Buttons.ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            clickClearButton();

            AddAssert("second binding cleared", () => string.IsNullOrEmpty(backBindingRow.Buttons.ElementAt(1).Text.Text));

            void clickClearButton()
            {
                AddStep("click clear button", () =>
                {
                    var clearButton = backBindingRow.ChildrenOfType<KeyBindingRow.ClearButton>().Single();

                    InputManager.MoveMouseTo(clearButton);
                    InputManager.Click(MouseButton.Left);
                });
            }
        }

        [Test]
        public void TestClickRowSelectsFirstBinding()
        {
            KeyBindingRow backBindingRow = null;

            AddStep("click back binding row", () =>
            {
                backBindingRow = panel.ChildrenOfType<KeyBindingRow>().ElementAt(10);
                InputManager.MoveMouseTo(backBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first binding selected", () => backBindingRow.Buttons.First().IsBinding);

            AddStep("click second binding", () =>
            {
                var target = backBindingRow.Buttons.ElementAt(1);

                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("click back binding row", () =>
            {
                backBindingRow = panel.ChildrenOfType<KeyBindingRow>().ElementAt(10);
                InputManager.MoveMouseTo(backBindingRow);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first binding selected", () => backBindingRow.Buttons.First().IsBinding);
        }
    }
}
