// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract partial class LinedPositionSnapGrid : PositionSnapGrid
    {
        protected void GenerateGridLines(Vector2 step, Vector2 drawSize)
        {
            if (Precision.AlmostEquals(step, Vector2.Zero))
                return;

            int index = 0;

            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;
            float rotation = MathHelper.RadiansToDegrees(MathF.Atan2(step.Y, step.X));

            List<Box> generatedLines = new List<Box>();

            while (true)
            {
                Vector2 currentPosition = StartPosition.Value + index++ * step;

                if (!lineDefinitelyIntersectsBox(currentPosition, step.PerpendicularLeft, drawSize, out var p1, out var p2))
                {
                    if (!isMovingTowardsBox(currentPosition, step, drawSize))
                        break;

                    continue;
                }

                var gridLine = new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.1f,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = lineWidth,
                    Height = Vector2.Distance(p1, p2),
                    Position = (p1 + p2) / 2,
                    Rotation = rotation,
                };

                generatedLines.Add(gridLine);
            }

            if (generatedLines.Count == 0)
                return;

            generatedLines.First().Alpha = 0.2f;

            AddRangeInternal(generatedLines);
        }

        private bool isMovingTowardsBox(Vector2 currentPosition, Vector2 step, Vector2 box)
        {
            return (currentPosition + step).LengthSquared < currentPosition.LengthSquared ||
                   (currentPosition + step - box).LengthSquared < (currentPosition - box).LengthSquared;
        }

        /// <summary>
        /// Determines if the line starting at <paramref name="lineStart"/> and going in the direction of <paramref name="lineDir"/>
        /// definitely intersects the box on (0, 0) with the given width and height and returns the intersection points if it does.
        /// </summary>
        /// <param name="lineStart">The start point of the line.</param>
        /// <param name="lineDir">The direction of the line.</param>
        /// <param name="box">The width and height of the box.</param>
        /// <param name="p1">The first intersection point.</param>
        /// <param name="p2">The second intersection point.</param>
        /// <returns>Whether the line definitely intersects the box.</returns>
        private bool lineDefinitelyIntersectsBox(Vector2 lineStart, Vector2 lineDir, Vector2 box, out Vector2 p1, out Vector2 p2)
        {
            p1 = Vector2.Zero;
            p2 = Vector2.Zero;

            if (Precision.AlmostEquals(lineDir.X, 0))
            {
                // If the line is vertical, we only need to check if the X coordinate of the line is within the box.
                if (!Precision.DefinitelyBigger(lineStart.X, 0) || !Precision.DefinitelyBigger(box.X, lineStart.X))
                    return false;

                p1 = new Vector2(lineStart.X, 0);
                p2 = new Vector2(lineStart.X, box.Y);
                return true;
            }

            if (Precision.AlmostEquals(lineDir.Y, 0))
            {
                // If the line is horizontal, we only need to check if the Y coordinate of the line is within the box.
                if (!Precision.DefinitelyBigger(lineStart.Y, 0) || !Precision.DefinitelyBigger(box.Y, lineStart.Y))
                    return false;

                p1 = new Vector2(0, lineStart.Y);
                p2 = new Vector2(box.X, lineStart.Y);
                return true;
            }

            float m = lineDir.Y / lineDir.X;
            float mInv = lineDir.X / lineDir.Y; // Use this to improve numerical stability if X is close to zero.
            float b = lineStart.Y - m * lineStart.X;

            // Calculate intersection points with the sides of the box.
            var p = new List<Vector2>(4);

            if (0 <= b && b <= box.Y)
                p.Add(new Vector2(0, b));
            if (0 <= (box.Y - b) * mInv && (box.Y - b) * mInv <= box.X)
                p.Add(new Vector2((box.Y - b) * mInv, box.Y));
            if (0 <= m * box.X + b && m * box.X + b <= box.Y)
                p.Add(new Vector2(box.X, m * box.X + b));
            if (0 <= -b * mInv && -b * mInv <= box.X)
                p.Add(new Vector2(-b * mInv, 0));

            switch (p.Count)
            {
                case 4:
                    // If there are 4 intersection points, the line is a diagonal of the box.
                    if (m > 0)
                    {
                        p1 = Vector2.Zero;
                        p2 = box;
                    }
                    else
                    {
                        p1 = new Vector2(0, box.Y);
                        p2 = new Vector2(box.X, 0);
                    }

                    break;

                case 3:
                    // If there are 3 intersection points, the line goes through a corner of the box.
                    if (p[0] == p[1])
                    {
                        p1 = p[0];
                        p2 = p[2];
                    }
                    else
                    {
                        p1 = p[0];
                        p2 = p[1];
                    }

                    break;

                case 2:
                    p1 = p[0];
                    p2 = p[1];

                    break;
            }

            return !Precision.AlmostEquals(p1, p2);
        }
    }
}
