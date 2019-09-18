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
            : base(getStringRepresentation(button))
        {
            Button = button;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private static string getStringRepresentation(MouseButton button)
        {
            switch (button)
            {
                default:
                    return button.ToString();

                case MouseButton.Left:
                    return @"M1";

                case MouseButton.Right:
                    return @"M2";
            }
        }

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
