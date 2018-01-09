// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [Description("player pause/fail screens")]
    public class TestCaseGameplayMenuOverlay : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(FailOverlay), typeof(PauseContainer) };

        private FailOverlay failOverlay;
        private PauseContainer.PauseOverlay pauseOverlay;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(pauseOverlay = new PauseContainer.PauseOverlay
            {
                OnResume = () => Logger.Log(@"Resume"),
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit"),
            });

            Add(failOverlay = new FailOverlay
            {
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit"),
            });

            var retryCount = 0;

            AddStep("Add retry", () =>
            {
                retryCount++;
                pauseOverlay.Retries = failOverlay.Retries = retryCount;
            });

            AddToggleStep("Toggle pause overlay", t => pauseOverlay.ToggleVisibility());
            AddToggleStep("Toggle fail overlay", t => failOverlay.ToggleVisibility());

            testHideResets();

            testEnterWithoutSelection();
            testKeyUpFromInitial();
            testKeyDownFromInitial();
            testKeyUpWrapping();
            testKeyDownWrapping();

            testMouseSelectionAfterKeySelection();
            testKeySelectionAfterMouseSelection();

            testMouseDeselectionResets();

            testClickSelection();
            testEnterKeySelection();
        }

        /// <summary>
        /// Test that hiding the overlay after hovering a button will reset the overlay to the initial state with no buttons selected.
        /// </summary>
        private void testHideResets()
        {
            AddStep("Show overlay", () => failOverlay.Show());

            AddStep("Hover first button", () => failOverlay.Buttons.First().TriggerOnMouseMove(null));
            AddStep("Hide overlay", () => failOverlay.Hide());

            AddAssert("Overlay state is reset", () => !failOverlay.Buttons.Any(b => b.Selected));
        }

        /// <summary>
        /// Tests that pressing enter after an overlay shows doesn't trigger an event because a selection hasn't occurred.
        /// </summary>
        private void testEnterWithoutSelection()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            AddStep("Press enter", () => pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Enter }));
            AddAssert("Overlay still open", () => pauseOverlay.State == Visibility.Visible);

            AddStep("Hide overlay", () => pauseOverlay.Hide());
        }

        /// <summary>
        /// Tests that pressing the up arrow from the initial state selects the last button.
        /// </summary>
        private void testKeyUpFromInitial()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            AddStep("Up arrow", () => pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Up }));
            AddAssert("Last button selected", () => pauseOverlay.Buttons.Last().Selected);

            AddStep("Hide overlay", () => pauseOverlay.Hide());
        }

        /// <summary>
        /// Tests that pressing the down arrow from the initial state selects the first button.
        /// </summary>
        private void testKeyDownFromInitial()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            AddStep("Down arrow", () => pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down }));
            AddAssert("First button selected", () => pauseOverlay.Buttons.First().Selected);

            AddStep("Hide overlay", () => pauseOverlay.Hide());
        }

        /// <summary>
        /// Tests that pressing the up arrow repeatedly causes the selected button to wrap correctly.
        /// </summary>
        private void testKeyUpWrapping()
        {
            AddStep("Show overlay", () => failOverlay.Show());

            AddStep("Up arrow", () => failOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Up }));
            AddAssert("Last button selected", () => failOverlay.Buttons.Last().Selected);
            AddStep("Up arrow", () => failOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Up }));
            AddAssert("First button selected", () => failOverlay.Buttons.First().Selected);
            AddStep("Up arrow", () => failOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Up }));
            AddAssert("Last button selected", () => failOverlay.Buttons.Last().Selected);

            AddStep("Hide overlay", () => failOverlay.Hide());
        }

        /// <summary>
        /// Tests that pressing the down arrow repeatedly causes the selected button to wrap correctly.
        /// </summary>
        private void testKeyDownWrapping()
        {
            AddStep("Show overlay", () => failOverlay.Show());

            AddStep("Down arrow", () => failOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down }));
            AddAssert("First button selected", () => failOverlay.Buttons.First().Selected);
            AddStep("Down arrow", () => failOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down }));
            AddAssert("Last button selected", () => failOverlay.Buttons.Last().Selected);
            AddStep("Down arrow", () => failOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down }));
            AddAssert("First button selected", () => failOverlay.Buttons.First().Selected);

            AddStep("Hide overlay", () => failOverlay.Hide());
        }

        /// <summary>
        /// Tests that hovering a button that was previously selected with the keyboard correctly selects the new button and deselects the previous button.
        /// </summary>
        private void testMouseSelectionAfterKeySelection()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            var secondButton = pauseOverlay.Buttons.Skip(1).First();

            AddStep("Down arrow", () => pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down }));
            AddStep("Hover second button", () => secondButton.TriggerOnMouseMove(null));
            AddAssert("First button not selected", () => !pauseOverlay.Buttons.First().Selected);
            AddAssert("Second button selected", () => secondButton.Selected);

            AddStep("Hide overlay", () => pauseOverlay.Hide());
        }

        /// <summary>
        /// Tests that pressing a key after selecting a button with a hover event correctly selects a new button and deselects the previous button.
        /// </summary>
        private void testKeySelectionAfterMouseSelection()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            var secondButton = pauseOverlay.Buttons.Skip(1).First();

            AddStep("Hover second button", () => secondButton.TriggerOnMouseMove(null));
            AddStep("Up arrow", () => pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Up }));
            AddAssert("Second button not selected", () => !secondButton.Selected);
            AddAssert("First button selected", () => pauseOverlay.Buttons.First().Selected);

            AddStep("Hide overlay", () => pauseOverlay.Hide());
        }

        /// <summary>
        /// Tests that deselecting with the mouse by losing hover will reset the overlay to the initial state.
        /// </summary>
        private void testMouseDeselectionResets()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            var secondButton = pauseOverlay.Buttons.Skip(1).First();

            AddStep("Hover second button", () => secondButton.TriggerOnMouseMove(null));
            AddStep("Unhover second button", () => secondButton.TriggerOnHoverLost(null));
            AddStep("Down arrow", () => pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down }));
            AddAssert("First button selected", () => pauseOverlay.Buttons.First().Selected); // Initial state condition

            AddStep("Hide overlay", () => pauseOverlay.Hide());
        }

        /// <summary>
        /// Tests that clicking on a button correctly causes a click event for that button.
        /// </summary>
        private void testClickSelection()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            var retryButton = pauseOverlay.Buttons.Skip(1).First();

            bool triggered = false;
            AddStep("Click retry button", () =>
            {
                var lastAction = pauseOverlay.OnRetry;
                pauseOverlay.OnRetry = () => triggered = true;

                retryButton.TriggerOnClick();
                pauseOverlay.OnRetry = lastAction;
            });

            AddAssert("Action was triggered", () => triggered);
            AddAssert("Overlay is closed", () => pauseOverlay.State == Visibility.Hidden);
        }

        /// <summary>
        /// Tests that pressing the enter key with a button selected correctly causes a click event for that button.
        /// </summary>
        private void testEnterKeySelection()
        {
            AddStep("Show overlay", () => pauseOverlay.Show());

            AddStep("Select second button", () =>
            {
                pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down });
                pauseOverlay.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Down });
            });

            var retryButton = pauseOverlay.Buttons.Skip(1).First();

            bool triggered = false;
            AddStep("Press enter", () =>
            {
                var lastAction = pauseOverlay.OnRetry;
                pauseOverlay.OnRetry = () => triggered = true;

                retryButton.TriggerOnKeyDown(null, new KeyDownEventArgs { Key = Key.Enter });
                pauseOverlay.OnRetry = lastAction;
            });

            AddAssert("Action was triggered", () => triggered);
            AddAssert("Overlay is closed", () => pauseOverlay.State == Visibility.Hidden);
        }
    }
}
