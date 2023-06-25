// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public partial class HotkeyExitOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (e.Action != GlobalAction.QuickExit) return false;

            BeginConfirm();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action != GlobalAction.QuickExit) return;

            AbortConfirm();
        }
    }
}
