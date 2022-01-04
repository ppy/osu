// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

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
            Height = 2f;

            AddInternal(new Box
            {
                Name = "Bar line",
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(255, 204, 33, 255),
            });

            if (barLine.Major)
            {
                AddInternal(new EquilateralTriangle
                {
                    Name = "Left triangle",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(triangle_height),
                    X = -triangle_offset,
                    Rotation = 90
                });

                AddInternal(new EquilateralTriangle
                {
                    Name = "Right triangle",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(triangle_height),
                    X = triangle_offset,
                    Rotation = -90
                });
            }

            if (!barLine.Major)
                Alpha = 0.2f;
        }

        protected override void UpdateInitialTransforms()
        {
        }

        protected override void UpdateStartTimeStateTransforms() => this.FadeOut(150);
    }
}
