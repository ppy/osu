// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneButtonSystemNavigation : OsuGameTestScene
    {
        private ButtonSystem buttons => ((MainMenu)Game.ScreenStack.CurrentScreen).ChildrenOfType<ButtonSystem>().Single();

        private GlobalActionContainer globalActionContainer => Game.ChildrenOfType<GlobalActionContainer>().First();

        [Test]
        public void TestKeyboardNavigation()
        {
            AddAssert("state is initial", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Initial));
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("state is top level", () => buttons.State, () => Is.EqualTo(ButtonSystemState.TopLevel));
            AddAssert("auto focused play button", () => buttons.CurrentSelection, () => Is.EqualTo(buttons.PlayButton));
            AddAssert("assume play button is 1 for following tests", () => buttons.SelectionIndex, () => Is.EqualTo(1));
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("button 2 is selected", () => buttons.SelectionIndex, () => Is.EqualTo(2));
            AddRepeatStep("press right (keybind) many times", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup), 7);
            AddAssert("last button is selected (exit button)", () => buttons.SelectionIndex, () => Is.EqualTo(4));
            AddRepeatStep("press left (keybind) 3 times", () => globalActionContainer.TriggerPressed(GlobalAction.SelectPreviousGroup), 3);
            AddAssert("auto focused play button", () => buttons.CurrentSelection, () => Is.EqualTo(buttons.PlayButton));
            AddStep("confirm selection", () => globalActionContainer.TriggerPressed(GlobalAction.Select));
            AddAssert("state is play buttons", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Play));
            AddAssert("solo button is selected", () => buttons.SelectionIndex, () => Is.EqualTo(1));
            AddStep("escape", () => globalActionContainer.TriggerPressed(GlobalAction.Back));
            AddAssert("play button is selected", () => buttons.SelectionIndex, () => Is.EqualTo(1));
            AddAssert("state is top level", () => buttons.State, () => Is.EqualTo(ButtonSystemState.TopLevel));
            AddStep("confirm selection", () => globalActionContainer.TriggerPressed(GlobalAction.Select));
            AddAssert("state is play buttons", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Play));
            AddStep("confirm selection", () => globalActionContainer.TriggerPressed(GlobalAction.Select));
            AddAssert("entered song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
        }

        [Test]
        public void TestKeyboardNavigationMouseInterrupted()
        {
            AddAssert("state is initial", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Initial));
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("state is top level", () => buttons.State, () => Is.EqualTo(ButtonSystemState.TopLevel));
            AddAssert("auto focused first button", () => buttons.SelectionIndex, () => Is.EqualTo(1));
            AddAssert("settings button looks not hovered", () => !buttons.CurrentButtonsList![0].LooksHovered);
            AddAssert("first button looks hovered", () => buttons.CurrentButtonsList![1].LooksHovered);
            AddAssert("second button looks not hovered", () => !buttons.CurrentButtonsList![2].LooksHovered);

            AddStep("hover second button", () => InputManager.MoveMouseTo(buttons.CurrentButtonsList![2]));
            AddAssert("first button looks not hovered", () => !buttons.CurrentButtonsList![1].LooksHovered);
            AddAssert("second button looks hovered", () => buttons.CurrentButtonsList![2].LooksHovered);
            AddAssert("third button looks not hovered", () => !buttons.CurrentButtonsList![3].LooksHovered);
            AddAssert("keyboard focus is gone", () => buttons.SelectionIndex, () => Is.EqualTo(null));

            AddStep("unhover buttons", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("first button looks not hovered", () => !buttons.CurrentButtonsList![1].LooksHovered);
            AddAssert("second button looks not hovered", () => !buttons.CurrentButtonsList![2].LooksHovered);
            AddAssert("third button looks not hovered", () => !buttons.CurrentButtonsList![3].LooksHovered);

            AddStep("hover third button", () => InputManager.MoveMouseTo(buttons.CurrentButtonsList![3]));
            AddAssert("keyboard focus is gone", () => buttons.SelectionIndex, () => Is.EqualTo(null));
            AddAssert("first button looks not hovered", () => !buttons.CurrentButtonsList![1].LooksHovered);
            AddAssert("second button looks not hovered", () => !buttons.CurrentButtonsList![2].LooksHovered);
            AddAssert("third button looks hovered", () => buttons.CurrentButtonsList![3].LooksHovered);

            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            // we reset keyboard focus when hovering - for the usecase to use a taiko controller for navigation, this feels most natural
            AddAssert("focused first button", () => buttons.SelectionIndex, () => Is.EqualTo(1));
            AddAssert("first button looks hovered", () => buttons.CurrentButtonsList![1].LooksHovered);
            AddAssert("second button looks not hovered", () => !buttons.CurrentButtonsList![2].LooksHovered);
            AddAssert("third button looks not hovered", () => !buttons.CurrentButtonsList![3].LooksHovered);
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("focused second button", () => buttons.SelectionIndex, () => Is.EqualTo(2));
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("focused third button", () => buttons.SelectionIndex, () => Is.EqualTo(3));
            AddStep("press left (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectPreviousGroup));
            AddStep("press left (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectPreviousGroup));
            AddAssert("focused first button", () => buttons.SelectionIndex, () => Is.EqualTo(1));
            AddStep("hover first button", () => InputManager.MoveMouseTo(buttons.CurrentButtonsList![1]));
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("state is play buttons", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Play));
        }

        [Test]
        public void TestGlobalActionHasPriority()
        {
            AddAssert("state is initial", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Initial));

            // triggering the cookie in the initial state with any key should only happen if no other action is bound to that key.
            // here, F10 is bound to GlobalAction.ToggleGameplayMouseButtons.
            AddStep("press F10", () => InputManager.Key(Key.F10));
            AddAssert("state is initial", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Initial));

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("state is top level", () => buttons.State, () => Is.EqualTo(ButtonSystemState.TopLevel));
        }

        [Test]
        public void TestShortcutKeys()
        {
            AddAssert("state is initial", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Initial));

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("state is top level", () => buttons.State, () => Is.EqualTo(ButtonSystemState.TopLevel));

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("state is play", () => buttons.State, () => Is.EqualTo(ButtonSystemState.Play));

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("entered song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
        }
    }
}
