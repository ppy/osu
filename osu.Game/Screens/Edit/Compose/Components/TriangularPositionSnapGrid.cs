// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class TriangularPositionSnapGrid : LinedPositionSnapGrid
    {
        private float spacing = 1;

        /// <summary>
        /// The spacing between grid lines of this <see cref="TriangularPositionSnapGrid"/>.
        /// </summary>
        public float Spacing
        {
            get => spacing;
            set
            {
                if (spacing <= 0)
                    throw new ArgumentException("Grid spacing must be positive.");

                spacing = value;
                GridCache.Invalidate();
            }
        }

        private float gridLineRotation;

        /// <summary>
        /// The rotation in degrees of the grid lines of this <see cref="TriangularPositionSnapGrid"/>.
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

        public TriangularPositionSnapGrid(Vector2 startPosition)
            : base(startPosition)
        {
        }

        private const float sqrt3 = 1.73205080757f;
        private const float sqrt3_over2 = 0.86602540378f;
        private const float one_over_sqrt3 = 0.57735026919f;

        protected override void CreateContent()
        {
            var drawSize = DrawSize;
            float stepSpacing = Spacing * sqrt3_over2;
            var step1 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation - 30);
            var step2 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation - 90);
            var step3 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation - 150);

            GenerateGridLines(step1, drawSize);
            GenerateGridLines(-step1, drawSize);

            GenerateGridLines(step2, drawSize);
            GenerateGridLines(-step2, drawSize);

            GenerateGridLines(step3, drawSize);
            GenerateGridLines(-step3, drawSize);

            GenerateOutline(drawSize);
        }

        public override Vector2 GetSnappedPosition(Vector2 original)
        {
            Vector2 relativeToStart = GeometryUtils.RotateVector(original - StartPosition, GridLineRotation);
            Vector2 hex = pixelToHex(relativeToStart);

            return StartPosition + GeometryUtils.RotateVector(hexToPixel(hex), -GridLineRotation);
        }

        private Vector2 pixelToHex(Vector2 pixel)
        {
            float x = pixel.X / Spacing;
            float y = pixel.Y / Spacing;
            // Algorithm from Charles Chambers
            // with modifications and comments by Chris Cox 2023
            // <https://gitlab.com/chriscox/hex-coordinates>
            float t = sqrt3 * y + 1; // scaled y, plus phase
            float temp1 = MathF.Floor(t + x); // (y+x) diagonal, this calc needs floor
            float temp2 = t - x; // (y-x) diagonal, no floor needed
            float temp3 = 2 * x + 1; // scaled horizontal, no floor needed, needs +1 to get correct phase
            float qf = (temp1 + temp3) / 3.0f; // pseudo x with fraction
            float rf = (temp1 + temp2) / 3.0f; // pseudo y with fraction
            float q = MathF.Floor(qf); // pseudo x, quantized and thus requires floor
            float r = MathF.Floor(rf); // pseudo y, quantized and thus requires floor
            return new Vector2(q, r);
        }

        private Vector2 hexToPixel(Vector2 hex)
        {
            // Taken from <https://www.redblobgames.com/grids/hexagons/#hex-to-pixel>
            // with modifications for the different definition of size.
            return new Vector2(Spacing * (hex.X - hex.Y / 2), Spacing * one_over_sqrt3 * 1.5f * hex.Y);
        }
    }
}
