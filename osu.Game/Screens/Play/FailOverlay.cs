// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.Pause;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Game.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : InGameOverlay
    {
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                if (State == Visibility.Hidden) return false;
                OnQuit();
                return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
