// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Screens.Play
{
    public partial class KeyCounterKeyboard : KeyCounter.Trigger
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
                Lit();

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == Key)
                Unlit();

            base.OnKeyUp(e);
        }
    }
}
