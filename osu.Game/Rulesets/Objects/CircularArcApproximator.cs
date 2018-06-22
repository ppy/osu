// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Game.Rulesets.Objects
{
    public class CircularArcApproximator
    {
        private readonly Vector2 a;
        private readonly Vector2 b;
        private readonly Vector2 c;

        private int amountPoints;

        private const float tolerance = 0.1f;

        public CircularArcApproximator(Vector2 a, Vector2 b, Vector2 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        /// <summary>
        /// Creates a piecewise-linear approximation of a circular arc curve.
        /// </summary>
        /// <returns>A list of vectors representing the piecewise-linear approximation.</returns>
        public List<Vector2> CreateArc()
        {
            float aSq = (b - c).LengthSquared;
            float bSq = (a - c).LengthSquared;
            float cSq = (a - b).LengthSquared;

            // If we have a degenerate triangle where a side-length is almost zero, then give up and fall
            // back to a more numerically stable method.
            if (Precision.AlmostEquals(aSq, 0) || Precision.AlmostEquals(bSq, 0) || Precision.AlmostEquals(cSq, 0))
                return new List<Vector2>();

            float s = aSq * (bSq + cSq - aSq);
            float t = bSq * (aSq + cSq - bSq);
            float u = cSq * (aSq + bSq - cSq);

            float sum = s + t + u;

            // If we have a degenerate triangle with an almost-zero size, then give up and fall
            // back to a more numerically stable method.
            if (Precision.AlmostEquals(sum, 0))
                return new List<Vector2>();

            Vector2 centre = (s * a + t * b + u * c) / sum;
            Vector2 dA = a - centre;
            Vector2 dC = c - centre;

            float r = dA.Length;

            double thetaStart = Math.Atan2(dA.Y, dA.X);
            double thetaEnd = Math.Atan2(dC.Y, dC.X);

            while (thetaEnd < thetaStart)
                thetaEnd += 2 * Math.PI;

            double dir = 1;
            double thetaRange = thetaEnd - thetaStart;

            // Decide in which direction to draw the circle, depending on which side of
            // AC B lies.
            Vector2 orthoAtoC = c - a;
            orthoAtoC = new Vector2(orthoAtoC.Y, -orthoAtoC.X);
            if (Vector2.Dot(orthoAtoC, b - a) < 0)
            {
                dir = -dir;
                thetaRange = 2 * Math.PI - thetaRange;
            }

            // We select the amount of points for the approximation by requiring the discrete curvature
            // to be smaller than the provided tolerance. The exact angle required to meet the tolerance
            // is: 2 * Math.Acos(1 - TOLERANCE / r)
            // The special case is required for extremely short sliders where the radius is smaller than
            // the tolerance. This is a pathological rather than a realistic case.
            amountPoints = 2 * r <= tolerance ? 2 : Math.Max(2, (int)Math.Ceiling(thetaRange / (2 * Math.Acos(1 - tolerance / r))));

            List<Vector2> output = new List<Vector2>(amountPoints);

            for (int i = 0; i < amountPoints; ++i)
            {
                double fract = (double)i / (amountPoints - 1);
                double theta = thetaStart + dir * fract * thetaRange;
                Vector2 o = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * r;
                output.Add(centre + o);
            }

            return output;
        }
    }
}
