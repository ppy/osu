// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Screens.Play.HUD
{
    public partial class KeyCounterKeyboardTrigger : InputTrigger
    {
        public Key Key { get; }

        public KeyCounterKeyboardTrigger(Key key)
            : base(key.ToString())
        {
            Key = key;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key)
            {
                Activate();
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == Key)
                Deactivate();

            base.OnKeyUp(e);
        }
    }
}
