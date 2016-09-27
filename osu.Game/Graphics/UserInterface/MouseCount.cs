//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class MouseCount : Count
    {
        public MouseButton Button { get; private set; }

        public MouseCount(String header, MouseButton button)
        {
            Header = header;
            Button = button;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == Button)
            {
                IsLit = true;
            }
            return false;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == Button)
            {
                IsLit = false;
            }
            return false;
        }
    }
}
