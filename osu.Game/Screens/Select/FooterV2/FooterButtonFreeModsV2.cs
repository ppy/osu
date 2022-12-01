// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonFreeModsV2 : FooterButtonV2
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            //No design exists for this button!
            Icon = FontAwesome.Solid.ExpandArrowsAlt;
            Text = "Freemods";
            AccentColour = colour.Yellow;
        }
    }
}
