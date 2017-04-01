﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Graphics.UserInterface.Volume
{
    internal class VolumeControlReceptor : Container
    {
        public Action<InputState> ActionRequested;

        protected override bool OnWheel(InputState state)
        {
            ActionRequested?.Invoke(state);
            return true;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Up:
                case Key.Down:
                    ActionRequested?.Invoke(state);
                    return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
