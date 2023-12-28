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
            Matrix2

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
                vector.X * MathF.Cos(MathUtils.DegreesToRadians(angle)) + vector.Y * MathF.Sin(MathUtils.DegreesToRadians(angle)),
                vector.X * -MathF.Sin(MathUtils.DegreesToRadians(angle)) + vector.Y * MathF.Cos(MathUtils.DegreesToRadians(angle))
            );
        }

        /// <summary>
        /// Given a flip direction, a surrounding quad for all selected objects, and a position,
        /// will return the flipped position in screen space coordinates.
        /// </summary>
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
            GetSurroundingQuad(hitObjects.SelectMany(h =>
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
            }));
    }
}
