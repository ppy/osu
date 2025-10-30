// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
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
        /// <param name="startAndEndOnly">Whether to only include the start and end positions of the slider, or include every control point in the slider.</param>
        public static Quad GetSurroundingQuad(IEnumerable<IHasPosition> hitObjects, bool startAndEndOnly = false) =>
            GetSurroundingQuad(startAndEndOnly ? enumerateStartAndEndPositions(hitObjects) : enumeratePositions(hitObjects));

        /// <summary>
        /// Returns the points that make up the convex hull of the provided points.
        /// </summary>
        /// <param name="points">The points to calculate a convex hull.</param>
        public static List<Vector2> GetConvexHull(IEnumerable<Vector2> points)
        {
            var pointsList = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            if (pointsList.Count < 3)
                return pointsList;

            var convexHullLower = new List<Vector2>
            {
                pointsList[0],
                pointsList[1]
            };
            var convexHullUpper = new List<Vector2>
            {
                pointsList[^1],
                pointsList[^2]
            };

            // Build the lower hull.
            for (int i = 2; i < pointsList.Count; i++)
            {
                Vector2 c = pointsList[i];
                while (convexHullLower.Count > 1 && isClockwise(convexHullLower[^2], convexHullLower[^1], c))
                    convexHullLower.RemoveAt(convexHullLower.Count - 1);

                convexHullLower.Add(c);
            }

            // Build the upper hull.
            for (int i = pointsList.Count - 3; i >= 0; i--)
            {
                Vector2 c = pointsList[i];
                while (convexHullUpper.Count > 1 && isClockwise(convexHullUpper[^2], convexHullUpper[^1], c))
                    convexHullUpper.RemoveAt(convexHullUpper.Count - 1);

                convexHullUpper.Add(c);
            }

            convexHullLower.RemoveAt(convexHullLower.Count - 1);
            convexHullUpper.RemoveAt(convexHullUpper.Count - 1);

            convexHullLower.AddRange(convexHullUpper);

            return convexHullLower;

            float crossProduct(Vector2 v1, Vector2 v2) => v1.X * v2.Y - v1.Y * v2.X;

            bool isClockwise(Vector2 a, Vector2 b, Vector2 c) => crossProduct(b - a, c - a) >= 0;
        }

        public static List<Vector2> GetConvexHull(IEnumerable<IHasPosition> hitObjects) =>
            GetConvexHull(enumeratePositions(hitObjects));

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

        private static IEnumerable<Vector2> enumeratePositions(IEnumerable<IHasPosition> hitObjects) =>
            hitObjects.SelectMany(h =>
            {
                if (h is IHasPath path)
                {
                    return path.Path.ControlPoints.Select(p => h.Position + p.Position);
                }

                return new[] { h.Position };
            });

        #region Welzl helpers

        // Function to check whether a point lies inside or on the boundaries of the circle
        private static bool isInside((Vector2 Centre, float Radius) c, Vector2 p)
        {
            return Precision.AlmostBigger(c.Radius, Vector2.Distance(c.Centre, p));
        }

        // Function to return a unique circle that intersects three points
        private static (Vector2, float) circleFrom(Vector2 a, Vector2 b, Vector2 c)
        {
            if (Precision.AlmostEquals(0, (b.Y - a.Y) * (c.X - a.X) - (b.X - a.X) * (c.Y - a.Y)))
                return circleFrom(a, b);

            // See: https://en.wikipedia.org/wiki/Circumcircle#Cartesian_coordinates
            float d = 2 * (a.X * (b - c).Y + b.X * (c - a).Y + c.X * (a - b).Y);
            float aSq = a.LengthSquared;
            float bSq = b.LengthSquared;
            float cSq = c.LengthSquared;

            var centre = new Vector2(
                aSq * (b - c).Y + bSq * (c - a).Y + cSq * (a - b).Y,
                aSq * (c - b).X + bSq * (a - c).X + cSq * (b - a).X) / d;

            return (centre, Vector2.Distance(a, centre));
        }

        // Function to return the smallest circle that intersects 2 points
        private static (Vector2, float) circleFrom(Vector2 a, Vector2 b)
        {
            var centre = (a + b) / 2.0f;
            return (centre, Vector2.Distance(a, b) / 2.0f);
        }

        // Function to check whether a circle encloses the given points
        private static bool isValidCircle((Vector2, float) c, List<Vector2> points)
        {
            // Iterating through all the points to check whether the points lie inside the circle or not
            foreach (Vector2 p in points)
            {
                if (!isInside(c, p)) return false;
            }

            return true;
        }

        // Function to return the minimum enclosing circle for N <= 3
        private static (Vector2, float) minCircleTrivial(List<Vector2> points)
        {
            if (points.Count > 3)
                throw new ArgumentException("Number of points must be at most 3", nameof(points));

            switch (points.Count)
            {
                case 0:
                    return (new Vector2(0, 0), 0);

                case 1:
                    return (points[0], 0);

                case 2:
                    return circleFrom(points[0], points[1]);
            }

            // To check if MEC can be determined by 2 points only
            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 3; j++)
                {
                    var c = circleFrom(points[i], points[j]);

                    if (isValidCircle(c, points))
                        return c;
                }
            }

            return circleFrom(points[0], points[1], points[2]);
        }

        #endregion

        /// <summary>
        /// Function to find the minimum enclosing circle for a collection of points.
        /// </summary>
        /// <returns>A tuple containing the circle centre and radius.</returns>
        public static (Vector2, float) MinimumEnclosingCircle(IEnumerable<Vector2> points)
        {
            // Using Welzl's algorithm to find the minimum enclosing circle
            // https://www.geeksforgeeks.org/minimum-enclosing-circle-using-welzls-algorithm/
            List<Vector2> p = points.ToList();

            var stack = new Stack<(Vector2?, int)>();
            var r = new List<Vector2>(3);
            (Vector2, float) d = (Vector2.Zero, 0);

            stack.Push((null, p.Count));

            while (stack.Count > 0)
            {
                // `n` represents the number of points in P that are not yet processed.
                // `point` represents the point that was randomly picked to process.
                (Vector2? point, int n) = stack.Pop();

                if (!point.HasValue)
                {
                    // Base case when all points processed or |R| = 3
                    if (n == 0 || r.Count == 3)
                    {
                        d = minCircleTrivial(r);
                        continue;
                    }

                    // Pick a random point randomly
                    int idx = RNG.Next(n);
                    point = p[idx];

                    // Put the picked point at the end of P since it's more efficient than
                    // deleting from the middle of the list
                    (p[idx], p[n - 1]) = (p[n - 1], p[idx]);

                    // Schedule processing of p after we get the MEC circle d from the set of points P - {p}
                    stack.Push((point, n));
                    // Get the MEC circle d from the set of points P - {p}
                    stack.Push((null, n - 1));
                }
                else
                {
                    // If d contains p, return d
                    if (isInside(d, point.Value))
                        continue;

                    // Remove points from R that were added in a deeper recursion
                    // |R| = |P| - |stack| - n
                    int removeCount = r.Count - (p.Count - stack.Count - n);
                    r.RemoveRange(r.Count - removeCount, removeCount);

                    // Otherwise, must be on the boundary of the MEC
                    r.Add(point.Value);
                    // Return the MEC for P - {p} and R U {p}
                    stack.Push((null, n - 1));
                }
            }

            return d;
        }

        /// <summary>
        /// Function to find the minimum enclosing circle for a collection of hit objects.
        /// </summary>
        /// <returns>A tuple containing the circle centre and radius.</returns>
        public static (Vector2, float) MinimumEnclosingCircle(IEnumerable<IHasPosition> hitObjects) =>
            MinimumEnclosingCircle(enumerateStartAndEndPositions(hitObjects));
    }
}
