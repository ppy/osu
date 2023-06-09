// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Input.Bindings;
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
            AddAssert("state is initial", () => buttons.State == ButtonSystemState.Initial);
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("state is top level", () => buttons.State == ButtonSystemState.TopLevel);
            AddAssert("auto focused first button", () => buttons.CurrentSelection == buttons.PlayButton);
            AddAssert("assume first button is 1 for following tests", () => buttons.SelectionIndex == 1);
            AddStep("press right (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup));
            AddAssert("button 2 is selected", () => buttons.SelectionIndex == 2);
            AddStep("press right (keybind) many times", () => {
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectNextGroup);
            });
            AddAssert("last button is selected (exit button)", () => buttons.SelectionIndex == 4);
            AddStep("press left (keybind) 3 times", () => {
                globalActionContainer.TriggerPressed(GlobalAction.SelectPreviousGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectPreviousGroup);
                globalActionContainer.TriggerPressed(GlobalAction.SelectPreviousGroup);
            });
            AddAssert("auto focused first button", () => buttons.CurrentSelection == buttons.PlayButton);
            AddStep("press down (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNext));
            AddAssert("state is play buttons", () => buttons.State == ButtonSystemState.Play);
            AddStep("press down (keybind)", () => globalActionContainer.TriggerPressed(GlobalAction.SelectNext));
            AddAssert("entered song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
        }

        [Test]
        public void TestGlobalActionHasPriority()
        {
            AddAssert("state is initial", () => buttons.State == ButtonSystemState.Initial);

            // triggering the cookie in the initial state with any key should only happen if no other action is bound to that key.
            // here, F10 is bound to GlobalAction.ToggleGameplayMouseButtons.
            AddStep("press F10", () => InputManager.Key(Key.F10));
            AddAssert("state is initial", () => buttons.State == ButtonSystemState.Initial);

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("state is top level", () => buttons.State == ButtonSystemState.TopLevel);
        }

        [Test]
        public void TestShortcutKeys()
        {
            AddAssert("state is initial", () => buttons.State == ButtonSystemState.Initial);

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("state is top level", () => buttons.State == ButtonSystemState.TopLevel);

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("state is play", () => buttons.State == ButtonSystemState.Play);

            AddStep("press P", () => InputManager.Key(Key.P));
            AddAssert("entered song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
        }
    }
}
