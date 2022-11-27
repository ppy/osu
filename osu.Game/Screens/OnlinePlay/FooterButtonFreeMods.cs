// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreeMods : FooterButton

    {
        [BackgroundDependencyLoader]
        private void load()
        {
            //TODO: no design for freemod button provided
            IconUsageBox = FontAwesome.Solid.ExpandArrowsAlt;
            ButtonAccentColour = Colour4.FromHex("FFCC22");
            Text = @"Freemods";
        }
    }
}
