// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class WedgeBackground : Container
    {
        public WedgeBackground()
        {
            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1, 0.5f),
                    Colour = Color4.Black,
                    Shear = new Vector2(0.15f, 0),
                    EdgeSmoothness = new Vector2(2, 0),
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Y,
                    Size = new Vector2(1, -0.5f),
                    Position = new Vector2(0, 1),
                    Colour = Color4.Black,
                    Shear = new Vector2(-0.15f, 0),
                    EdgeSmoothness = new Vector2(2, 0),
                },
            };
        }
    }
}
