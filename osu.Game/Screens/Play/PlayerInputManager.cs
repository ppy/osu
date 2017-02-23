// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Game.Configuration;
using System;
using System.Linq;

namespace osu.Game.Screens.Play
{
    class PlayerInputManager : UserInputManager
    {
        public PlayerInputManager(GameHost host)
            : base(host)
        {
        }

        bool leftViaKeyboard;
        bool rightViaKeyboard;
        Bindable<bool> mouseDisabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuConfig.MouseDisableButtons)
                ?? new Bindable<bool>(false);
        }

        protected override void TransformState(InputState state)
        {
            base.TransformState(state);

            if (state.Keyboard != null)
            {
                leftViaKeyboard = state.Keyboard.Keys.Contains(Key.Z);
                rightViaKeyboard = state.Keyboard.Keys.Contains(Key.X);
            }

            var mouse = (Framework.Input.MouseState)state.Mouse;
            if (state.Mouse != null)
            {
                if (mouseDisabled.Value)
                {
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Left).State = false;
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Right).State = false;
                }

                if (leftViaKeyboard)
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Left).State = true;
                if (rightViaKeyboard)
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Right).State = true;
            }
        }
    }
}
