// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Screens.Play
{
    public class KeyCounterKeyboard : KeyCounter
    {
        public Key Key { get; }

        public KeyCounterKeyboard(Key key)
            : base(key.ToString())
        {
            Key = key;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key)
            {
                IsLit = true;
                Increment();
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == Key) IsLit = false;
            return base.OnKeyUp(e);
        }
    }
}
