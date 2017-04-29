// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Input;
using osu.Game.Screens.Play;
using OpenTK.Input;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Game.Rulesets.Osu
{
    public class OsuKeyConversionInputManager : KeyConversionInputManager
    {
        private bool leftViaKeyboard;
        private bool rightViaKeyboard;

        protected override void TransformState(InputState state)
        {
            base.TransformState(state);

            var mouse = state.Mouse as MouseState;
            var keyboard = state.Keyboard as KeyboardState;

            if (keyboard != null)
            {
                leftViaKeyboard = keyboard.Keys.Contains(Key.Z);
                rightViaKeyboard = keyboard.Keys.Contains(Key.X);
            }

            if (mouse != null)
            {
                if (leftViaKeyboard)
                    mouse.SetPressed(MouseButton.Left, true);
                if (rightViaKeyboard)
                    mouse.SetPressed(MouseButton.Right, true);
            }
        }
    }
}
