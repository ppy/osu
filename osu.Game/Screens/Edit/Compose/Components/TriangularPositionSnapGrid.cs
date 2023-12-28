// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class TriangularPositionSnapGrid : CompositeDrawable
    {
        private Vector2 startPosition;

        /// <summary>
        /// The position of the origin of this <see cref="TriangularPositionSnapGrid"/> in local coordinates.
        /// </summary>
        public Vector2 StartPosition
        {
            get => startPosition;
            set
            {
                startPosition = value;
                gridCache.Invalidate();
            }
        }

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
                gridCache.Invalidate();
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
                gridCache.Invalidate();
            }
        }

        private readonly LayoutValue gridCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

        public TriangularPositionSnapGrid(Vector2 startPosition)
        {
            StartPosition = startPosition;
            Masking = true;

            AddLayout(gridCache);
        }

        protected override void Update()
        {
            base.Update();

            if (!gridCache.IsValid)
            {
                ClearInternal();

                if (DrawWidth > 0 && DrawHeight > 0)
                    createContent();

                gridCache.Validate();
            }
        }

        private const float sqrt3 = 1.73205080757f;
        private const float sqrt3_over2 = 0.86602540378f;
        private const float one_over_sqrt3 = 0.57735026919f;

        private void createContent()
        {
            var drawSize = DrawSize;
            float stepSpacing = Spacing * sqrt3_over2;
            var step1 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation - 30);
            var step2 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation - 90);
            var step3 = GeometryUtils.RotateVector(new Vector2(stepSpacing, 0), -GridLineRotation - 150);

            generateGridLines(step1, drawSize);
            generateGridLines(-step1, drawSize);

            generateGridLines(step2, drawSize);
            generateGridLines(-step2, drawSize);

            generateGridLines(step3, drawSize);
            generateGridLines(-step3, drawSize);

            generateOutline(drawSize);
        }

        private void generateGridLines(Vector2 step, Vector2 drawSize)
        {
            int index = 0;
            var currentPosition = startPosition;

            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;
            float lineLength = drawSize.Length * 2;

            List<Box> generatedLines = new List<Box>();

            while (lineDefinitelyIntersectsBox(currentPosition, step.PerpendicularLeft, drawSize) ||
                   isMovingTowardsBox(currentPosition, step, drawSize))
            {
                var gridLine = new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.1f,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = lineWidth,
                    Height = lineLength,
                    Position = currentPosition,
                    Rotation = MathHelper.RadiansToDegrees(MathF.Atan2(step.Y, step.X)),
                };

                generatedLines.Add(gridLine);

                index += 1;
                currentPosition = startPosition + index * step;
            }

            if (generatedLines.Count == 0)
                return;

            generatedLines.First().Alpha = 0.3f;

            AddRangeInternal(generatedLines);
        }

        private bool isMovingTowardsBox(Vector2 currentPosition, Vector2 step, Vector2 box)
        {
            return (currentPosition + step).LengthSquared < currentPosition.LengthSquared ||
                   (currentPosition + step - box).LengthSquared < (currentPosition - box).LengthSquared;
        }

        private bool lineDefinitelyIntersectsBox(Vector2 lineStart, Vector2 lineDir, Vector2 box)
        {
            var p2 = lineStart + lineDir;

            double d1 = det(Vector2.Zero);
            double d2 = det(new Vector2(box.X, 0));
            double d3 = det(new Vector2(0, box.Y));
            double d4 = det(box);

            return definitelyDifferentSign(d1, d2) || definitelyDifferentSign(d3, d4) ||
                   definitelyDifferentSign(d1, d3) || definitelyDifferentSign(d2, d4);

            double det(Vector2 p) => (p.X - lineStart.X) * (p2.Y - lineStart.Y) - (p.Y - lineStart.Y) * (p2.X - lineStart.X);

            bool definitelyDifferentSign(double a, double b) => !Precision.AlmostEquals(a, 0) &&
                                                                !Precision.AlmostEquals(b, 0) &&
                                                                Math.Sign(a) != Math.Sign(b);
        }

        private void generateOutline(Vector2 drawSize)
        {
            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;

            AddRangeInternal(new[]
            {
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = lineWidth,
                    Y = 0,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = lineWidth,
                    Y = drawSize.Y,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Width = lineWidth,
                    X = 0,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Width = lineWidth,
                    X = drawSize.X,
                },
            });
        }

        public Vector2 GetSnappedPosition(Vector2 original)
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
            return new Vector2(Spacing * (hex.X - hex.Y / 2), Spacing * one_over_sqrt3 * 1.5f * hex.Y);
        }
    }
}
