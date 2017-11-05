// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class KeyCounterKeyboard : KeyCounter, IHandleOnKeyDown, IHandleOnKeyUp
    {
        public Key Key { get; }
        public KeyCounterKeyboard(Key key) : base(key.ToString())
        {
            Key = key;
        }

        public virtual bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key) IsLit = true;
            return false;
        }

        public virtual bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == Key) IsLit = false;
            return false;
        }
    }
}
