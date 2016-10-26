//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;
using OpenTK;

namespace osu.Game.Graphics.UserInterface.Volume
{
    class VolumeControlReceptor : Container
    {
        public Action ActivateRequested;

        protected override bool OnWheelDown(InputState state)
        {
            ActivateRequested?.Invoke();
            return true;
        }

        protected override bool OnWheelUp(InputState state)
        {
            ActivateRequested?.Invoke();
            return true;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Up:
                case Key.Down:
                    ActivateRequested?.Invoke();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
