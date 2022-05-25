// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Overlays.Mods
{
    public class SelectAllModsButton : ShearedButton, IKeyBindingHandler<PlatformAction>
    {
        public SelectAllModsButton(FreeModSelectOverlay modSelectOverlay)
            : base(ModSelectOverlay.BUTTON_WIDTH)
        {
            Text = CommonStrings.SelectAll;
            Action = modSelectOverlay.SelectAll;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat || e.Action != PlatformAction.SelectAll)
                return false;

            TriggerClick();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }
}
