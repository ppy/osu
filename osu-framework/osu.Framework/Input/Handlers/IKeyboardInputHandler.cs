//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;

namespace osu.Framework.Input.Handlers
{
    interface IKeyboardInputHandler
    {
        List<Key> PressedKeys { get; }
    }
}
