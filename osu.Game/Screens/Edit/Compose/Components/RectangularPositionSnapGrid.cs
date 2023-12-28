// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class RectangularPositionSnapGrid : LinedPositionSnapGrid
    {
        private Vector2 spacing = Vector2.One;

        /// <summary>
        /// The spacing between grid lines of this <see cref="RectangularPositionSnapGrid"/>.
        /// </summary>
        public Vector2 Spacing
        {
            get => spacing;
            set
            {
                if (spacing.X <= 0 || spacing.Y <= 0)
                    throw new ArgumentException("Grid spacing must be positive.");

                spacing = value;
                GridCache.Invalidate();
            }
        }

        private float gridLineRotation;

        /// <summary>
        /// The rotation in degrees of the grid lines of this <see cref="RectangularPositionSnapGrid"/>.
        /// </summary>
        public float GridLineRotation
        {
            get => gridLineRotation;
            set
            {
                gridLineRotation = value;
                GridCache.Invalidate();
            }
        }

        protected override void CreateContent()
        {
            var drawSize = DrawSize;
            var rot = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(GridLineRotation));

            GenerateGridLines(Vector2.Transform(new Vector2(0, -Spacing.Y), rot), drawSize);
            GenerateGridLines(Vector2.Transform(new Vector2(0, Spacing.Y), rot), drawSize);

            GenerateGridLines(Vector2.Transform(new Vector2(-Spacing.X, 0), rot), drawSize);
            GenerateGridLines(Vector2.Transform(new Vector2(Spacing.X, 0), rot), drawSize);

            GenerateOutline(drawSize);
        }

        public override Vector2 GetSnappedPosition(Vector2 original)
        {
            Vector2 relativeToStart = GeometryUtils.RotateVector(original - StartPosition, GridLineRotation);
            Vector2 offset = Vector2.Divide(relativeToStart, Spacing);
            Vector2 roundedOffset = new Vector2(MathF.Round(offset.X), MathF.Round(offset.Y));

            return StartPosition + GeometryUtils.RotateVector(Vector2.Multiply(roundedOffset, Spacing), -GridLineRotation);
        }
    }
}
