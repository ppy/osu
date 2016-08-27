//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Linq;
using OpenTK.Input;
using osu.Framework.Lists;

namespace osu.Framework.Input
{
    public class KeyboardState
    {
        public KeyboardState LastState;

        public ReadOnlyList<Key> Keys = new ReadOnlyList<Key>();

        public KeyboardState(KeyboardState last = null)
        {
            LastState = last;
        }

        public bool ControlPressed => Keys.Contains(Key.LControl) || Keys.Contains(Key.RControl);
        public bool AltPressed => Keys.Contains(Key.LAlt) || Keys.Contains(Key.RAlt);
        public bool ShiftPressed => Keys.Contains(Key.LShift) || Keys.Contains(Key.RShift);
    }
}
