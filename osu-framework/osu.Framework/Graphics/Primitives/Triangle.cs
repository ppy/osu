//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace osu.Framework.Graphics.Primitives
{
    public struct Triangle
    {
        public Vector2 P0;
        public Vector2 P1;
        public Vector2 P2;

        public Triangle(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
        }

        /// <summary>
        /// Checks whether a point lies within the triangle.
        /// </summary>
        /// <param name="pos">The point to check.</param>
        /// <returns>Outcome of the check.</returns>
        public Vector2? Contains(Vector2 pos)
        {
            // This uses barycentric coordinates with slight simplifications for faster computation.
            // See: https://en.wikipedia.org/wiki/Barycentric_coordinate_system
            float area2 = (P0.Y * (P2.X - P1.X) + P0.X * (P1.Y - P2.Y) + P1.X * P2.Y - P1.Y * P2.X);
            if (area2 == 0)
                return null;

            float s = (P0.Y * P2.X - P0.X * P2.Y + (P2.Y - P0.Y) * pos.X + (P0.X - P2.X) * pos.Y) / area2;
            if (s < 0)
                return null;

            float t = (P0.X * P1.Y - P0.Y * P1.X + (P0.Y - P1.Y) * pos.X + (P1.X - P0.X) * pos.Y) / area2;
            if (t < 0 || (s + t) > 1)
                return null;

            return new Vector2(s, t);
        }
    }
}
