// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osuTK.Input;
using osuTK;

namespace osu.Game.Screens.Play
{
    public class KeyCounterMouse : KeyCounter
    {
        public MouseButton Button { get; }

        public KeyCounterMouse(MouseButton button)
            : base(button switch
            {
                MouseButton.Left => @"M1",
                MouseButton.Right => @"M2",
                _ => button.ToString(),
            })
        {
            Button = button;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == Button)
            {
                IsLit = true;
                Increment();
            }

            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            if (e.Button == Button) IsLit = false;
            return base.OnMouseUp(e);
        }
    }
}
