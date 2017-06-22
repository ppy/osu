// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="BarLine"/>. Although this derives DrawableManiaHitObject,
    /// this does not handle input/sound like a normal hit object.
    /// </summary>
    public class DrawableBarLine : DrawableManiaHitObject<BarLine>
    {
        /// <summary>
        /// Height of major bar line triangles.
        /// </summary>
        private const float triangle_height = 12;

        /// <summary>
        /// Offset of the major bar line triangles from the sides of the bar line.
        /// </summary>
        private const float triangle_offset = 9;

        public DrawableBarLine(BarLine barLine)
            : base(barLine)
        {
            RelativeSizeAxes = Axes.X;
            Height = 1;

            Add(new Box
            {
                Name = "Bar line",
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
            });

            bool isMajor = barLine.BeatIndex % (int)barLine.ControlPoint.TimeSignature == 0;

            if (isMajor)
            {
                Add(new EquilateralTriangle
                {
                    Name = "Left triangle",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(triangle_height),
                    X = -triangle_offset,
                    Rotation = 90
                });

                Add(new EquilateralTriangle
                {
                    Name = "Right triangle",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(triangle_height),
                    X = triangle_offset,
                    Rotation = -90
                });
            }

            if (!isMajor && barLine.BeatIndex % 2 == 1)
                Alpha = 0.2f;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}