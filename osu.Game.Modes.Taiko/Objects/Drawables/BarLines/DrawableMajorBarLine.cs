// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Modes.Taiko.Objects.Drawables.BarLines
{
    public class DrawableMajorBarLine : DrawableBarLine
    {
        private Container arrows;

        public DrawableMajorBarLine(BarLine barLine)
            : base(barLine)
        {
            Add(arrows = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                RelativeSizeAxes = Axes.Both,

                Children = new[]
                {
                    // Top
                    new EquilateralTriangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, -10),
                        EdgeSmoothness = new Vector2(1),
                        Size = new Vector2(20, -20),
                    },
                    // Bottom
                    new EquilateralTriangle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, 10),
                        EdgeSmoothness = new Vector2(1),
                        Size = new Vector2(20),
                    }
                }
            });

            Tracker.Alpha = 1f;
        }
    }
}
