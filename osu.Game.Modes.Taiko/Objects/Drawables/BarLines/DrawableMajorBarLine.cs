// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Taiko.Objects.Drawables.BarLines
{
    public class DrawableMajorBarLine : DrawableBarLine
    {
        public DrawableMajorBarLine(BarLine barLine)
            : base(barLine)
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                RelativeSizeAxes = Axes.Both,

                Children = new[]
                {
                    // Top
                    new Triangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, -10),
                        EdgeSmoothness = new Vector2(1),
                        // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                        Size = new Vector2(20, 0.866f * -20),
                    },
                    // Bottom
                    new Triangle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, 10),
                        EdgeSmoothness = new Vector2(1),
                        // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                        Size = new Vector2(20, 0.866f * 20),
                    }
                }
            });

            Tracker.Alpha = 1f;
        }
    }
}
