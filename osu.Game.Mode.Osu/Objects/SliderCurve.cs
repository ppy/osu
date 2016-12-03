using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.Diagnostics;
using osu.Framework.MathUtils;
using System;

namespace osu.Game.Modes.Osu.Objects
{
    public class SliderCurve
    {
        public double Length;

        public List<Vector2> Path;

        public CurveTypes CurveType;

        private List<Vector2> calculatedPath = new List<Vector2>();
        private List<double> cumulativeLength = new List<double>();

        private List<Vector2> calculateSubpath(List<Vector2> subpath)
        {
            switch (CurveType)
            {
                case CurveTypes.Linear:
                    return subpath;
                default:
                    return new BezierApproximator(subpath).CreateBezier();
            }
        }

        public void Calculate()
        {
            calculatedPath.Clear();

            // Sliders may consist of various subpaths separated by two consecutive vertices
            // with the same position. The following loop parses these subpaths and computes
            // their shape independently, consecutively appending them to calculatedPath.
            List<Vector2> subpath = new List<Vector2>();
            for (int i = 0; i < Path.Count; ++i)
            {
                subpath.Add(Path[i]);
                if (i == Path.Count - 1 || Path[i] == Path[i + 1])
                {
                    // If we already constructed a subpath previously, then the new subpath
                    // will have as starting position the end position of the previous subpath.
                    // Hence we can and should remove the previous endpoint to avoid a segment
                    // with 0 length.
                    if (calculatedPath.Count > 0)
                        calculatedPath.RemoveAt(calculatedPath.Count - 1);

                    calculatedPath.AddRange(calculateSubpath(subpath));
                    subpath.Clear();
                }
            }
            
            cumulativeLength.Clear();
            cumulativeLength.Add(Length = 0);
            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                double d = (calculatedPath[i + 1] - calculatedPath[i]).Length;

                Debug.Assert(d >= 0, "Cumulative lengths have to be strictly increasing.");
                cumulativeLength.Add(Length += d);
            }
        }

        private int indexOfDistance(double d)
        {
            int i = cumulativeLength.BinarySearch(d);
            if (i < 0) i = ~i;

            return i;
        }

        private double progressToDistance(double progress)
        {
            return MathHelper.Clamp(progress, 0, 1) * Length;
        }

        private Vector2 interpolateVertices(int i, double d)
        {
            if (calculatedPath.Count == 0)
                return Vector2.Zero;

            if (i <= 0)
                return calculatedPath.First();
            else if (i >= calculatedPath.Count)
                return calculatedPath.Last();

            Vector2 p0 = calculatedPath[i - 1];
            Vector2 p1 = calculatedPath[i];

            double d0 = cumulativeLength[i - 1];
            double d1 = cumulativeLength[i];

            // Avoid division by and almost-zero number in case two points are extremely close to each other.
            if (Precision.AlmostEquals(d0, d1))
                return p0;

            double w = (d - d0) / (d1 - d0);
            return p0 + (p1 - p0) * (float)w;
        }

        /// <summary>
        /// Computes the slider curve until a given progress that ranges from 0 (beginning of the slider)
        /// to 1 (end of the slider) and stores the generated path in the given list.
        /// </summary>
        /// <param name="path">The list to be filled with the computed curve.</param>
        /// <param name="progress">Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        public void GetPathToProgress(List<Vector2> path, double p0, double p1)
        {
            double d0 = progressToDistance(p0);
            double d1 = progressToDistance(p1);

            path.Clear();

            int i = 0;
            for (; i < calculatedPath.Count && cumulativeLength[i] < d0; ++i);

            path.Add(interpolateVertices(i, d0));

            for (; i < calculatedPath.Count && cumulativeLength[i] <= d1; ++i)
                path.Add(calculatedPath[i]);

            path.Add(interpolateVertices(i, d1));
        }

        /// <summary>
        /// Computes the position on the slider at a given progress that ranges from 0 (beginning of the slider)
        /// to 1 (end of the slider).
        /// </summary>
        /// <param name="progress">Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        /// <returns></returns>
        public Vector2 PositionAt(double progress)
        {
            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d);
        }
    }
}