//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.MathUtils;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects
{
    public class CircularArcApproximator
    {
        private Vector2 A;
        private Vector2 B;
        private Vector2 C;

        private int amountPoints;

        private const float TOLERANCE = 0.1f;

        public CircularArcApproximator(Vector2 A, Vector2 B, Vector2 C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        /// <summary>
        /// Creates a piecewise-linear approximation of a circular arc curve.
        /// </summary>
        /// <returns>A list of vectors representing the piecewise-linear approximation.</returns>
        public List<Vector2> CreateArc()
        {
            float aSq = (B - C).LengthSquared;
            float bSq = (A - C).LengthSquared;
            float cSq = (A - B).LengthSquared;

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

            Vector2 centre = (s * A + t * B + u * C) / sum;
            Vector2 dA = A - centre;
            Vector2 dC = C - centre;

            float r = dA.Length;

            double thetaStart = Math.Atan2(dA.Y, dA.X);
            double thetaEnd = Math.Atan2(dC.Y, dC.X);

            while (thetaEnd < thetaStart)
                thetaEnd += 2 * Math.PI;

            double dir = 1;
            double thetaRange = thetaEnd - thetaStart;

            // Decide in which direction to draw the circle, depending on which side of 
            // AC B lies.
            Vector2 orthoAC = C - A;
            orthoAC = new Vector2(orthoAC.Y, -orthoAC.X);
            if (Vector2.Dot(orthoAC, B - A) < 0)
            {
                dir = -dir;
                thetaRange = 2 * Math.PI - thetaRange;
            }

            // We select the amount of points for the approximation by requiring the discrete curvature
            // to be smaller than the provided tolerance. The exact angle required to meet the tolerance
            // is: 2 * Math.Acos(1 - TOLERANCE / r)
            if (2 * r <= TOLERANCE)
                // This special case is required for extremely short sliders where the radius is smaller than
                // the tolerance. This is a pathological rather than a realistic case.
                amountPoints = 2;
            else
                amountPoints = Math.Max(2, (int)Math.Ceiling(thetaRange / (2 * Math.Acos(1 - TOLERANCE / r))));

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
