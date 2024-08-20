// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Utils
{
    public static class GeometryUtils
    {
        /// <summary>
        /// Rotate a point around an arbitrary origin.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="origin">The centre origin to rotate around.</param>
        /// <param name="angle">The angle to rotate (in degrees).</param>
        public static Vector2 RotatePointAroundOrigin(Vector2 point, Vector2 origin, float angle)
        {
            angle = -angle;

            point.X -= origin.X;
            point.Y -= origin.Y;

            Vector2 ret = RotateVector(point, angle);

            ret.X += origin.X;
            ret.Y += origin.Y;

            return ret;
        }

        /// <summary>
        /// Rotate a vector around the origin.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="angle">The angle to rotate (in degrees).</param>
        public static Vector2 RotateVector(Vector2 vector, float angle)
        {
            return new Vector2(
                vector.X * MathF.Cos(float.DegreesToRadians(angle)) + vector.Y * MathF.Sin(float.DegreesToRadians(angle)),
                vector.X * -MathF.Sin(float.DegreesToRadians(angle)) + vector.Y * MathF.Cos(float.DegreesToRadians(angle))
            );
        }

        /// <summary>
        /// Given a flip direction, a surrounding quad for all selected objects, and a position,
        /// will return the flipped position in screen space coordinates.
        /// </summary>
        /// <param name="direction">The direction to flip towards.</param>
        /// <param name="quad">The quad surrounding all selected objects. The center of this determines the position of the axis.</param>
        /// <param name="position">The position to flip.</param>
        public static Vector2 GetFlippedPosition(Direction direction, Quad quad, Vector2 position)
        {
            var centre = quad.Centre;

            switch (direction)
            {
                case Direction.Horizontal:
                    position.X = centre.X - (position.X - centre.X);
                    break;

                case Direction.Vertical:
                    position.Y = centre.Y - (position.Y - centre.Y);
                    break;
            }

            return position;
        }

        /// <summary>
        /// Given a flip axis vector, a surrounding quad for all selected objects, and a position,
        /// will return the flipped position in screen space coordinates.
        /// </summary>
        /// <param name="axis">The vector indicating the direction to flip towards. This is perpendicular to the mirroring axis.</param>
        /// <param name="quad">The quad surrounding all selected objects. The center of this determines the position of the axis.</param>
        /// <param name="position">The position to flip.</param>
        public static Vector2 GetFlippedPosition(Vector2 axis, Quad quad, Vector2 position)
        {
            var centre = quad.Centre;

            return position - 2 * Vector2.Dot(position - centre, axis) * axis;
        }

        /// <summary>
        /// Given a scale vector, a surrounding quad for all selected objects, and a position,
        /// will return the scaled position in screen space coordinates.
        /// </summary>
        public static Vector2 GetScaledPosition(Anchor reference, Vector2 scale, Quad selectionQuad, Vector2 position)
        {
            // adjust the direction of scale depending on which side the user is dragging.
            float xOffset = ((reference & Anchor.x0) > 0) ? -scale.X : 0;
            float yOffset = ((reference & Anchor.y0) > 0) ? -scale.Y : 0;

            // guard against no-ops and NaN.
            if (scale.X != 0 && selectionQuad.Width > 0)
                position.X = selectionQuad.TopLeft.X + xOffset + (position.X - selectionQuad.TopLeft.X) / selectionQuad.Width * (selectionQuad.Width + scale.X);

            if (scale.Y != 0 && selectionQuad.Height > 0)
                position.Y = selectionQuad.TopLeft.Y + yOffset + (position.Y - selectionQuad.TopLeft.Y) / selectionQuad.Height * (selectionQuad.Height + scale.Y);

            return position;
        }

        /// <summary>
        /// Given a scale multiplier, an origin, and a position,
        /// will return the scaled position in screen space coordinates.
        /// </summary>
        public static Vector2 GetScaledPosition(Vector2 scale, Vector2 origin, Vector2 position, float axisRotation = 0)
        {
            return origin + RotateVector(RotateVector(position - origin, axisRotation) * scale, -axisRotation);
        }

        /// <summary>
        /// Returns a quad surrounding the provided points.
        /// </summary>
        /// <param name="points">The points to calculate a quad for.</param>
        public static Quad GetSurroundingQuad(IEnumerable<Vector2> points)
        {
            if (!points.Any())
                return new Quad();

            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var p in points)
            {
                minPosition = Vector2.ComponentMin(minPosition, p);
                maxPosition = Vector2.ComponentMax(maxPosition, p);
            }

            Vector2 size = maxPosition - minPosition;

            return new Quad(minPosition.X, minPosition.Y, size.X, size.Y);
        }

        /// <summary>
        /// Returns a gamefield-space quad surrounding the provided hit objects.
        /// </summary>
        /// <param name="hitObjects">The hit objects to calculate a quad for.</param>
        public static Quad GetSurroundingQuad(IEnumerable<IHasPosition> hitObjects) =>
            GetSurroundingQuad(enumerateStartAndEndPositions(hitObjects));

        /// <summary>
        /// Returns the points that make up the convex hull of the provided points.
        /// </summary>
        /// <param name="points">The points to calculate a convex hull.</param>
        public static List<Vector2> GetConvexHull(IEnumerable<Vector2> points)
        {
            List<Vector2> p = points.ToList();

            if (p.Count < 3)
                return p;

            p.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            List<Vector2> upper = new List<Vector2>();
            List<Vector2> lower = new List<Vector2>();

            // Build the lower hull
            for (int i = 0; i < p.Count; i++)
            {
                while (lower.Count >= 2 && cross(lower[^2], lower[^1], p[i]) <= 0)
                    lower.RemoveAt(lower.Count - 1);

                lower.Add(p[i]);
            }

            // Build the upper hull
            for (int i = p.Count - 1; i >= 0; i--)
            {
                while (upper.Count >= 2 && cross(upper[^2], upper[^1], p[i]) <= 0)
                    upper.RemoveAt(upper.Count - 1);

                upper.Add(p[i]);
            }

            // Remove the last point of each half because it's a duplicate of the first point of the other half
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);

            lower.AddRange(upper);
            return lower;

            float cross(Vector2 o, Vector2 a, Vector2 b) => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }

        public static List<Vector2> GetConvexHull(IEnumerable<IHasPosition> hitObjects) =>
            GetConvexHull(enumerateStartAndEndPositions(hitObjects));

        private static IEnumerable<Vector2> enumerateStartAndEndPositions(IEnumerable<IHasPosition> hitObjects) =>
            hitObjects.SelectMany(h =>
            {
                if (h is IHasPath path)
                {
                    return new[]
                    {
                        h.Position,
                        // can't use EndPosition for reverse slider cases.
                        h.Position + path.Path.PositionAt(1)
                    };
                }

                return new[] { h.Position };
            });
    }
}
