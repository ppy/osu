//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;

namespace osu.Framework.Input.Handlers.Keyboard
{
    class OpenTKKeyboardHandler : InputHandler, IKeyboardInputHandler
    {
        public override bool IsActive => true;

        public override int Priority => 0;

        public override void Dispose()
        {
        }

        public override bool Initialize()
        {
            PressedKeys = new List<Key>();
            return true;
        }

        public override void UpdateInput(bool isActive)
        {
            OpenTK.Input.KeyboardState state = OpenTK.Input.Keyboard.GetState();

            PressedKeys.Clear();

            if (state.IsAnyKeyDown)
            {
                foreach (Key k in Enum.GetValues(typeof(Key)))
                {
                    if (state.IsKeyDown(k))
                        PressedKeys.Add(k);
                }
            }
        }

        public List<Key> PressedKeys { get; set; }
    }
}
