// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Mods;

namespace osu.Game.Screens.SelectV2.Footer
{
    public partial class ScreenFooterButtonFreeMods : ScreenFooterButtonMods
    {
        public ScreenFooterButtonFreeMods(ModSelectOverlay overlay)
            : base(overlay)
        {
            Text = "Free Mods";

            // TODO: this should look a bit different than the regular mods button
        }
    }
}
