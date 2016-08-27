//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using osu.Framework.Extensions.RectangleExtensions;
using osu.Framework.Graphics.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace osu.Framework.Extensions.PolygonExtensions
{
    /// <summary>
    /// Todo: Support segment containment and circles.
    /// Todo: Might be overkill, but possibly support convex decomposition? 
    /// </summary>
    public static class IConvexPolygonExtensions
    {
        /// <summary>
        /// Determines whether two convex polygons intersect.
        /// </summary>
        /// <param name="first">The first polygon.</param>
        /// <param name="second">The second polygon.</param>
        /// <returns>Whether the two polygons intersect.</returns>
        public static bool Intersects(this IConvexPolygon first, IConvexPolygon second)
        {
            Vector2[][] bothAxes = { first.GetAxes(), second.GetAxes() };

            Vector2[] firstVertices = first.Vertices;
            Vector2[] secondVertices = second.Vertices;

            foreach (Vector2[] axes in bothAxes)
            {
                foreach (Vector2 axis in axes)
                {
                    ProjectionRange firstRange = new ProjectionRange(axis, firstVertices);
                    ProjectionRange secondRange = new ProjectionRange(axis, secondVertices);

                    if (!firstRange.Overlaps(secondRange))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two convex polygons intersect.
        /// </summary>
        /// <param name="first">The first polygon.</param>
        /// <param name="second">The second polygon.</param>
        /// <returns>Whether the two polygons intersect.</returns>
        public static bool Intersects(this IConvexPolygon first, Rectangle second)
        {
            Vector2[][] bothAxes = { first.GetAxes(), second.GetAxes() };

            Vector2[] firstVertices = first.Vertices;
            Vector2[] secondVertices = second.GetVertices();

            foreach (Vector2[] axes in bothAxes)
            {
                foreach (Vector2 axis in axes)
                {
                    ProjectionRange firstRange = new ProjectionRange(axis, firstVertices);
                    ProjectionRange secondRange = new ProjectionRange(axis, secondVertices);

                    if (!firstRange.Overlaps(secondRange))
                        return false;
                }
            }

            return true;
        }
    }
}
