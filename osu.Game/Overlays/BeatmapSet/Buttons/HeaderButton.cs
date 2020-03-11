// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class HeaderButton : TriangleButton
    {
        public HeaderButton()
        {
            Height = 0;
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Color4Extensions.FromHex(@"094c5f");
            Triangles.ColourLight = Color4Extensions.FromHex(@"0f7c9b");
            Triangles.ColourDark = Color4Extensions.FromHex(@"094c5f");
            Triangles.TriangleScale = 1.5f;
        }
    }
}
