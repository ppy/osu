//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Extensions.PolygonExtensions
{
    public static class IPolygonExtensions
    {
        /// <summary>
        /// Computes the axes for each edge in a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to return the axes of.</param>
        /// <param name="normalize">Whether the normals should be normalized. Allows computation of the exact intersection point.</param>
        /// <returns>The axes of the polygon.</returns>
        public static Vector2[] GetAxes(this IPolygon polygon, bool normalize = false)
        {
            Vector2[] axes = new Vector2[polygon.AxisVertices.Length];

            for (int i = 0; i < polygon.AxisVertices.Length; i++)
            {
                // Construct an edge between two sequential points
                Vector2 v1 = polygon.AxisVertices[i];
                Vector2 v2 = polygon.AxisVertices[i == polygon.AxisVertices.Length - 1 ? 0 : i + 1];
                Vector2 edge = v2 - v1;

                // Find the normal to the edge
                Vector2 normal = new Vector2(-edge.Y, edge.X);

                if (normalize)
                    normal = Vector2.Normalize(normal);

                axes[i] = normal;
            }

            return axes;
        }
    }
}
