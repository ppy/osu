// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableBarLineMajor : DrawableBarLine
    {
        /// <summary>
        /// The vertical offset of the triangles from the line tracker.
        /// </summary>
        private const float triangle_offfset = 10f;

        /// <summary>
        /// The size of the triangles.
        /// </summary>
        private const float triangle_size = 20f;

        public DrawableBarLineMajor(BarLine barLine)
            : base(barLine)
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    new EquilateralTriangle
                    {
                        Name = "Top",
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, -triangle_offfset),
                        Size = new Vector2(-triangle_size),
                        EdgeSmoothness = new Vector2(1),
                    },
                    new EquilateralTriangle
                    {
                        Name = "Bottom",
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, triangle_offfset),
                        Size = new Vector2(triangle_size),
                        EdgeSmoothness = new Vector2(1),
                    }
                }
            });

            Tracker.Alpha = 1f;
        }
    }
}