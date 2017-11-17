﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public class KeyCounterMouse : KeyCounter, IHandleMouseButtons
    {
        public MouseButton Button { get; }

        public KeyCounterMouse(MouseButton button) : base(getStringRepresentation(button))
        {
            Button = button;
        }

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

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

        public virtual bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == Button) IsLit = true;
            return false;
        }

        public virtual bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == Button) IsLit = false;
            return false;
        }
    }
}
