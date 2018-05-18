// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Game.Overlays;
using OpenTK.Input;

namespace osu.Game.Screens.Menu
{
    public class ExitConfirmOverlay : HoldToConfirmOverlay
    {
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape && !args.Repeat)
            {
                BeginConfirm();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                AbortConfirm();
                return true;
            }

            return base.OnKeyUp(state, args);
        }
    }
}
