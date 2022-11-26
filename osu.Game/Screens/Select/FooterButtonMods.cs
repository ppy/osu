// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Select
{
    public class FooterButtonMods : FooterButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            //TODO: use https://fontawesome.com/icons/arrow-right-arrow-left?s=solid&f=classic when local Fontawesome is updated
            IconUsageBox = FontAwesome.Solid.ArrowsAlt;
            ButtonAccentColour = Colour4.FromHex("#B2FF66");
            Text = @"Mods";
            Hotkey = GlobalAction.ToggleModSelection;
        }
    }
}
