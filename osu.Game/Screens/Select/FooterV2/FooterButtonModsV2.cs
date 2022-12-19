// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonModsV2 : FooterButtonV2
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Text = "Mods";
            Icon = FontAwesome.Solid.ExchangeAlt;
            AccentColour = colour.Lime1;
        }
    }
}
