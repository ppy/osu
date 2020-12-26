// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Rooms.GameTypes
{
    public class VersusRow : FillFlowContainer
    {
        public VersusRow(Color4 first, Color4 second, float size)
        {
            var triangleSize = new Vector2(size);
            AutoSizeAxes = Axes.Both;
            Spacing = new Vector2(2f, 0f);

            Children = new[]
            {
                new Container
                {
                    Size = triangleSize,
                    Colour = first,
                    Children = new[]
                    {
                        new EquilateralTriangle
                        {
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Both,
                            Rotation = 90,
                            EdgeSmoothness = new Vector2(1f),
                        },
                    },
                },
                new Container
                {
                    Size = triangleSize,
                    Colour = second,
                    Children = new[]
                    {
                        new EquilateralTriangle
                        {
                            Anchor = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Both,
                            Rotation = -90,
                            EdgeSmoothness = new Vector2(1f),
                        },
                    },
                },
            };
        }
    }
}
