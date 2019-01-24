// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
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
            BackgroundColour = OsuColour.FromHex(@"094c5f");
            Triangles.ColourLight = OsuColour.FromHex(@"0f7c9b");
            Triangles.ColourDark = OsuColour.FromHex(@"094c5f");
            Triangles.TriangleScale = 1.5f;
        }
    }
}
