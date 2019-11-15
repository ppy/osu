// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("player pause/fail screens")]
    public class TestSceneGameplayMenuOverlay : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(FailOverlay), typeof(PauseOverlay) };

        private FailOverlay failOverlay;
        private PauseOverlay pauseOverlay;

        private GlobalActionContainer globalActionContainer;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            Child = globalActionContainer = new GlobalActionContainer(game);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            globalActionContainer.Children = new Drawable[]
            {
                pauseOverlay = new PauseOverlay
                {
                    OnResume = () => Logger.Log(@"Resume"),
                    OnRetry = () => Logger.Log(@"Retry"),
                    OnQuit = () => Logger.Log(@"Quit"),
                },
                failOverlay = new FailOverlay

                {
                    OnRetry = () => Logger.Log(@"Retry"),
                    OnQuit = () => Logger.Log(@"Quit"),
                }
            };

            InputManager.MoveMouseTo(Vector2.Zero);
        });

        [Test]
        public void TestAdjustRetryCount()
        {
            showOverlay();

            var retryCount = 0;

            AddRepeatStep("Add retry", () =>
            {
                retryCount++;
                pauseOverlay.Retries = failOverlay.Retries = retryCount;
            }, 10);
        }

        /// <summary>
        /// Tests that pressing enter after an overlay shows doesn't trigger an event because a selection hasn't occurred.
        /// </summary>
        [Test]
        public void TestEnterWithoutSelection()
        {
            showOverlay();

            AddStep("Press select", () => press(GlobalAction.Select));
            AddAssert("Overlay still open", () => pauseOverlay.State.Value == Visibility.Visible);
        }

        /// <summary>
        /// Tests that pressing the up arrow from the initial state selects the last button.
        /// </summary>
        [Test]
        public void TestKeyUpFromInitial()
        {
            showOverlay();

            AddStep("Up arrow", () => press(Key.Up));
            AddAssert("Last button selected", () => pauseOverlay.Buttons.Last().Selected.Value);
        }

        /// <summary>
        /// Tests that pressing the down arrow from the initial state selects the first button.
        /// </summary>
        [Test]
        public void TestKeyDownFromInitial()
        {
            showOverlay();

            AddStep("Down arrow", () => press(Key.Down));
            AddAssert("First button selected", () => getButton(0).Selected.Value);
        }

        /// <summary>
        /// Tests that pressing the up arrow repeatedly causes the selected button to wrap correctly.
        /// </summary>
        [Test]
        public void TestKeyUpWrapping()
        {
            AddStep("Show overlay", () => failOverlay.Show());

            AddStep("Up arrow", () => press(Key.Up));
            AddAssert("Last button selected", () => failOverlay.Buttons.Last().Selected.Value);
            AddStep("Up arrow", () => press(Key.Up));
            AddAssert("First button selected", () => failOverlay.Buttons.First().Selected.Value);
            AddStep("Up arrow", () => press(Key.Up));
            AddAssert("Last button selected", () => failOverlay.Buttons.Last().Selected.Value);
        }

        /// <summary>
        /// Tests that pressing the down arrow repeatedly causes the selected button to wrap correctly.
        /// </summary>
        [Test]
        public void TestKeyDownWrapping()
        {
            AddStep("Show overlay", () => failOverlay.Show());

            AddStep("Down arrow", () => press(Key.Down));
            AddAssert("First button selected", () => failOverlay.Buttons.First().Selected.Value);
            AddStep("Down arrow", () => press(Key.Down));
            AddAssert("Last button selected", () => failOverlay.Buttons.Last().Selected.Value);
            AddStep("Down arrow", () => press(Key.Down));
            AddAssert("First button selected", () => failOverlay.Buttons.First().Selected.Value);
        }

        /// <summary>
        /// Test that hiding the overlay after hovering a button will reset the overlay to the initial state with no buttons selected.
        /// </summary>
        [Test]
        public void TestHideResets()
        {
            AddStep("Show overlay", () => failOverlay.Show());

            AddStep("Hover first button", () => InputManager.MoveMouseTo(failOverlay.Buttons.First()));
            AddStep("Hide overlay", () => failOverlay.Hide());

            AddAssert("Overlay state is reset", () => !failOverlay.Buttons.Any(b => b.Selected.Value));
        }

        /// <summary>
        /// Tests that entering menu with cursor initially on button doesn't selects it immediately.
        /// This is to allow for stable keyboard navigation.
        /// </summary>
        [Test]
        public void TestInitialButtonHover()
        {
            showOverlay();

            AddStep("Hover first button", () => InputManager.MoveMouseTo(getButton(0)));

            AddStep("Hide overlay", () => pauseOverlay.Hide());
            showOverlay();

            AddAssert("First button not selected", () => !getButton(0).Selected.Value);

            AddStep("Move slightly", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(1)));

            AddAssert("First button selected", () => getButton(0).Selected.Value);
        }

        /// <summary>
        /// Tests that hovering a button that was previously selected with the keyboard correctly selects the new button and deselects the previous button.
        /// </summary>
        [Test]
        public void TestMouseSelectionAfterKeySelection()
        {
            showOverlay();

            AddStep("Down arrow", () => press(Key.Down));
            AddStep("Hover second button", () => InputManager.MoveMouseTo(getButton(1)));
            AddAssert("First button not selected", () => !getButton(0).Selected.Value);
            AddAssert("Second button selected", () => getButton(1).Selected.Value);
        }

        /// <summary>
        /// Tests that pressing a key after selecting a button with a hover event correctly selects a new button and deselects the previous button.
        /// </summary>
        [Test]
        public void TestKeySelectionAfterMouseSelection()
        {
            AddStep("Show overlay", () =>
            {
                pauseOverlay.Show();
            });

            AddStep("Hover second button", () => InputManager.MoveMouseTo(getButton(1)));
            AddStep("Up arrow", () => press(Key.Up));
            AddAssert("Second button not selected", () => !getButton(1).Selected.Value);
            AddAssert("First button selected", () => getButton(0).Selected.Value);
        }

        /// <summary>
        /// Tests that deselecting with the mouse by losing hover will reset the overlay to the initial state.
        /// </summary>
        [Test]
        public void TestMouseDeselectionResets()
        {
            showOverlay();

            AddStep("Hover second button", () => InputManager.MoveMouseTo(getButton(1)));
            AddStep("Unhover second button", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddStep("Down arrow", () => press(Key.Down));
            AddAssert("First button selected", () => getButton(0).Selected.Value); // Initial state condition
        }

        /// <summary>
        /// Tests that clicking on a button correctly causes a click event for that button.
        /// </summary>
        [Test]
        public void TestClickSelection()
        {
            showOverlay();

            bool triggered = false;
            AddStep("Click retry button", () =>
            {
                var lastAction = pauseOverlay.OnRetry;
                pauseOverlay.OnRetry = () => triggered = true;

                getButton(1).Click();
                pauseOverlay.OnRetry = lastAction;
            });

            AddAssert("Action was triggered", () => triggered);
            AddAssert("Overlay is closed", () => pauseOverlay.State.Value == Visibility.Hidden);
        }

        /// <summary>
        /// Tests that pressing the enter key with a button selected correctly causes a click event for that button.
        /// </summary>
        [Test]
        public void TestEnterKeySelection()
        {
            showOverlay();

            AddStep("Select second button", () =>
            {
                press(Key.Down);
                press(Key.Down);
            });

            bool triggered = false;
            Action lastAction = null;
            AddStep("Press enter", () =>
            {
                lastAction = pauseOverlay.OnRetry;
                pauseOverlay.OnRetry = () => triggered = true;
                press(Key.Enter);
            });

            AddAssert("Action was triggered", () =>
            {
                if (lastAction != null)
                {
                    pauseOverlay.OnRetry = lastAction;
                    lastAction = null;
                }

                return triggered;
            });
            AddAssert("Overlay is closed", () => pauseOverlay.State.Value == Visibility.Hidden);
        }

        private void showOverlay() => AddStep("Show overlay", () => pauseOverlay.Show());

        private DialogButton getButton(int index) => pauseOverlay.Buttons.Skip(index).First();

        private void press(Key key)
        {
            InputManager.PressKey(key);
            InputManager.ReleaseKey(key);
        }

        private void press(GlobalAction action)
        {
            globalActionContainer.TriggerPressed(action);
            globalActionContainer.TriggerReleased(action);
        }
    }
}
