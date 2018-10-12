// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Events;
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

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key) IsLit = true;
            return base.OnKeyDown(e);
        }

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == Key) IsLit = false;
            return base.OnKeyUp(e);
        }
    }
}
