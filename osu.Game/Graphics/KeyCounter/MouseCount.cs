using OpenTK;
using OpenTK.Input;
﻿// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;

namespace osu.Game.Graphics.KeyCounter
{
    /// <summary>
    ///  A counter targeted toward a specific Mouse button.
    ///  Note: Left/Right buttons supported only
    /// </summary>
    class MouseCount : Count
    {
        private OpenTK.Input.MouseButton eventMouse;

        internal MouseCount(string btnName, OpenTK.Input.MouseButton btn)
        {
            base.name = btnName;
            eventMouse = btn;
        }

        //public override bool HasFocus => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == eventMouse)
            {
                if (IsLit == false)
                    IsLit = true;
            }

            return false;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == eventMouse)
            {
                if (IsLit == true)
                    IsLit = false;
            }

            return false;
        }
    }
}