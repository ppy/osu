// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public class HotkeyExitOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        public bool OnPressed(GlobalAction action)
        {
            if (action != GlobalAction.QuickExit) return false;

            BeginConfirm();
            return true;
        }

        public bool OnReleased(GlobalAction action)
        {
            if (action != GlobalAction.QuickExit) return false;

            AbortConfirm();
            return true;
        }
    }
}
