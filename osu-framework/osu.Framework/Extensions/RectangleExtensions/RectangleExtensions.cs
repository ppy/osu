//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace osu.Framework.Extensions.RectangleExtensions
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// Computes the axes for each edge in a rectangle.
        /// <para>A rectangle has equal normals for opposing edges, so only two axes will be returned.</para>
        /// </summary>
        /// <param name="rectangle">The rectangle to return the axes of.</param>
        /// <param name="normalize">Whether the normals should be normalized. Allows computation of the exact intersection point.</param>
        /// <returns>The axes of the rectangle.</returns>
        public static Vector2[] GetAxes(this Rectangle rectangle, bool normalize = false)
        {
            Vector2[] edges = { new Vector2(rectangle.Right - rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom - rectangle.Top) };

            for (int i = 0; i < edges.Length; i++)
            {
                Vector2 normal = new Vector2(-edges[i].Y, edges[i].X);

                if (normalize)
                    normal = Vector2.Normalize(normal);

                edges[i] = normal;
            }

            return edges;
        }

        public static Vector2[] GetVertices(this Rectangle rectangle)
        {
            return new[]
            {
                new Vector2(rectangle.Left, rectangle.Top),
                new Vector2(rectangle.Right, rectangle.Top),
                new Vector2(rectangle.Right, rectangle.Bottom),
                new Vector2(rectangle.Left, rectangle.Bottom)
            };
        }
    }
}
