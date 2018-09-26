// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK.Input;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public class KeyCounterMouse : KeyCounter
    {
        public MouseButton Button { get; }

        public KeyCounterMouse(MouseButton button) : base(getStringRepresentation(button))
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

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == Button) IsLit = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == Button) IsLit = false;
            return base.OnMouseUp(state, args);
        }
    }
}
