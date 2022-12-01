// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonRandomV2 : FooterButtonV2
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Random";
            Icon = FontAwesome.Solid.Random;
            AccentColour = Colour4.FromHex("#66CCFF");
        }
    }
}
