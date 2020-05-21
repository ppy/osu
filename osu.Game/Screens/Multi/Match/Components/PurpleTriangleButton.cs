// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class PurpleTriangleButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Colour4.FromHex(@"593790");
            Triangles.ColourLight = Colour4.FromHex(@"7247b6");
            Triangles.ColourDark = Colour4.FromHex(@"593790");
        }
    }
}
