// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public class EditorHotkeyQuickExitContainer : TapToConfirmContainer, IKeyBindingHandler<GlobalAction>
    {
        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat || e.Action != GlobalAction.EditorTestQuickExit)
                return false;

            BeginConfirm();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
