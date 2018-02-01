// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Rulesets.Objects
{
    public class CatmullApproximator
    {
        /// <summary>
        /// The amount of pieces to calculate for each controlpoint quadruplet.
        /// </summary>
        private const int detail = 50;

        private readonly List<Vector2> controlPoints;

        public CatmullApproximator(List<Vector2> controlPoints)
        {
            this.controlPoints = controlPoints;
        }


        /// <summary>
        /// Creates a piecewise-linear approximation of a Catmull-Rom spline.
        /// </summary>
        /// <returns>A list of vectors representing the piecewise-linear approximation.</returns>
        public List<Vector2> CreateCatmull()
        {
            var result = new List<Vector2>();

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                var v1 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
                var v2 = controlPoints[i];
                var v3 = i < controlPoints.Count - 1 ? controlPoints[i + 1] : v2 + v2 - v1;
                var v4 = i < controlPoints.Count - 2 ? controlPoints[i + 2] : v3 + v3 - v2;

                for (int c = 0; c < detail; c++)
                {
                    result.Add(findPoint(ref v1, ref v2, ref v3, ref v4, (float)c / detail));
                    result.Add(findPoint(ref v1, ref v2, ref v3, ref v4, (float)(c + 1) / detail));
                }
            }

            return result;
        }

        /// <summary>
        /// Finds a point on the spline at the position of a parameter.
        /// </summary>
        /// <param name="vec1">The first vector.</param>
        /// <param name="vec2">The second vector.</param>
        /// <param name="vec3">The third vector.</param>
        /// <param name="vec4">The fourth vector.</param>
        /// <param name="t">The parameter at which to find the point on the spline, in the range [0, 1].</param>
        /// <returns>The point on the spline at <paramref name="t"/>.</returns>
        private Vector2 findPoint(ref Vector2 vec1, ref Vector2 vec2, ref Vector2 vec3, ref Vector2 vec4, float t)
        {
            float t2 = t * t;
            float t3 = t * t2;

            Vector2 result;
            result.X = 0.5f * (2f * vec2.X + (-vec1.X + vec3.X) * t + (2f * vec1.X - 5f * vec2.X + 4f * vec3.X - vec4.X) * t2 + (-vec1.X + 3f * vec2.X - 3f * vec3.X + vec4.X) * t3);
            result.Y = 0.5f * (2f * vec2.Y + (-vec1.Y + vec3.Y) * t + (2f * vec1.Y - 5f * vec2.Y + 4f * vec3.Y - vec4.Y) * t2 + (-vec1.Y + 3f * vec2.Y - 3f * vec3.Y + vec4.Y) * t3);

            return result;
        }
    }
}
