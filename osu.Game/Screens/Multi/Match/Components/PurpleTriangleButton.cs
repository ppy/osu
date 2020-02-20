// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class PurpleTriangleButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = OsuColour.FromHex(@"593790");
            Triangles.ColourLight = OsuColour.FromHex(@"7247b6");
            Triangles.ColourDark = OsuColour.FromHex(@"593790");
        }
    }
}