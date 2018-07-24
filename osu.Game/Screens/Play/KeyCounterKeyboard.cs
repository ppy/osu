// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class KeyCounterKeyboard : KeyCounter
    {
        public Key Key { get; }
        public KeyCounterKeyboard(Key key) : base(key.ToString())
        {
            Key = key;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key) IsLit = true;
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == Key) IsLit = false;
            return base.OnKeyUp(state, args);
        }
    }
}
