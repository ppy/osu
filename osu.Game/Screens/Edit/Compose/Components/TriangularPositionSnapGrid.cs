﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class TriangularPositionSnapGrid : LinedPositionSnapGrid
    {
        /// <summary>
        /// The spacing between grid lines of this <see cref="TriangularPositionSnapGrid"/>.
        /// </summary>
        public BindableFloat Spacing { get; } = new BindableFloat(1f)
        {
            MinValue = 0f,
        };

        /// <summary>
        /// The rotation in degrees of the grid lines of this <see cref="TriangularPositionSnapGrid"/>.
        /// </summary>
        public BindableFloat GridLineRotation { get; } = new BindableFloat();

        public TriangularPositionSnapGrid()
        {
            Spacing.BindValueChanged(_ => GridCache.Invalidate());
            GridLineRotation.BindValueChanged(_ => GridCache.Invalidate());
        }

        private static readonly float sqrt3 = float.Sqrt(3);
        private static readonly float sqrt3_over2 = sqrt3 / 2;
        private static readonly float one_over_sqrt3 = 1 / sqrt3;

        protected override void CreateContent()
        {
            var drawSize = DrawSize;
            float stepSpacing = Spacing.Value * sqrt3_over2;
            var step1 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation.Value - 30);
            var step2 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation.Value - 90);
            var step3 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation.Value - 150);

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
            Vector2 relativeToStart = GeometryUtils.RotateVector(original - StartPosition.Value, GridLineRotation.Value);
            Vector2 hex = pixelToHex(relativeToStart);

            return StartPosition.Value + GeometryUtils.RotateVector(hexToPixel(hex), -GridLineRotation.Value);
        }

        private Vector2 pixelToHex(Vector2 pixel)
        {
            float x = pixel.X / Spacing.Value;
            float y = pixel.Y / Spacing.Value;
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
            return new Vector2(Spacing.Value * (hex.X - hex.Y / 2), Spacing.Value * one_over_sqrt3 * 1.5f * hex.Y);
        }
    }
}
