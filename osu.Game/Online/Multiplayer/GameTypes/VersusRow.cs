// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Multiplayer.GameTypes
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
