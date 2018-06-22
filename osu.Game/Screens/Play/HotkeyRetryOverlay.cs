// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public class HotkeyRetryOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        public bool OnPressed(GlobalAction action)
        {
            if (action != GlobalAction.QuickRetry) return false;

            BeginConfirm();
            return true;
        }

        public bool OnReleased(GlobalAction action)
        {
            if (action != GlobalAction.QuickRetry) return false;

            AbortConfirm();
            return true;
        }
    }
}
