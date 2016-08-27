//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;

namespace osu.Framework.Input
{
    public class InputState : EventArgs
    {
        public KeyboardState Keyboard;
        public MouseState Mouse;

        public InputState(InputState last = null)
        {
            Keyboard = new KeyboardState(last?.Keyboard);
            Mouse = new MouseState(last?.Mouse);
        }
    }
}
