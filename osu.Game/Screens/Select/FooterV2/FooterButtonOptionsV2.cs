// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonOptionsV2 : FooterButtonV2
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Options";
            Icon = FontAwesome.Solid.Cog;
            AccentColour = Colour4.FromHex("#8C66FF");
        }
    }
}
