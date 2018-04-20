// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;

namespace osu.Game.Rulesets.Objects
{
    public class SliderCurve
    {
        public double Distance;

        public List<Vector2> ControlPoints;

        public CurveType CurveType = CurveType.PerfectCurve;

        public Vector2 Offset;

        private readonly List<Vector2> calculatedPath = new List<Vector2>();
        private readonly List<double> cumulativeLength = new List<double>();

        private List<Vector2> calculateSubpath(List<Vector2> subControlPoints)
        {
            switch (CurveType)
            {
                case CurveType.Linear:
                    return subControlPoints;
                case CurveType.PerfectCurve:
                    //we can only use CircularArc iff we have exactly three control points and no dissection.
                    if (ControlPoints.Count != 3 || subControlPoints.Count != 3)
                        break;

                    // Here we have exactly 3 control points. Attempt to fit a circular arc.
                    List<Vector2> subpath = new CircularArcApproximator(subControlPoints[0], subControlPoints[1], subControlPoints[2]).CreateArc();

                    // If for some reason a circular arc could not be fit to the 3 given points, fall back to a numerically stable bezier approximation.
                    if (subpath.Count == 0)
                        break;

                    return subpath;
                case CurveType.Catmull:
                    return new CatmullApproximator(subControlPoints).CreateCatmull();
            }

            return new BezierApproximator(subControlPoints).CreateBezier();
        }

        private void calculatePath()
        {
            calculatedPath.Clear();

            // Sliders may consist of various subpaths separated by two consecutive vertices
            // with the same position. The following loop parses these subpaths and computes
            // their shape independently, consecutively appending them to calculatedPath.
            List<Vector2> subControlPoints = new List<Vector2>();
            for (int i = 0; i < ControlPoints.Count; ++i)
            {
                subControlPoints.Add(ControlPoints[i]);
                if (i == ControlPoints.Count - 1 || ControlPoints[i] == ControlPoints[i + 1])
                {
                    List<Vector2> subpath = calculateSubpath(subControlPoints);
                    foreach (Vector2 t in subpath)
                        if (calculatedPath.Count == 0 || calculatedPath.Last() != t)
                            calculatedPath.Add(t);

                    subControlPoints.Clear();
                }
            }
        }

        private void calculateCumulativeLengthAndTrimPath()
        {
            double l = 0;

            cumulativeLength.Clear();
            cumulativeLength.Add(l);

            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                double d = diff.Length;

                // Shorten slider curves that are too long compared to what's
                // in the .osu file.
                if (Distance - l < d)
                {
                    calculatedPath[i + 1] = calculatedPath[i] + diff * (float)((Distance - l) / d);
                    calculatedPath.RemoveRange(i + 2, calculatedPath.Count - 2 - i);

                    l = Distance;
                    cumulativeLength.Add(l);
                    break;
                }

                l += d;
                cumulativeLength.Add(l);
            }

            //TODO: Figure out if the following code is needed in some cases. Judging by the map
            //      "Transform" http://osu.ppy.sh/s/484689 it seems like we should _not_ be doing this.
            // Lengthen slider curves that are too short compared to what's
            // in the .osu file.
            /*if (l < Length && calculatedPath.Count > 1)
            {
                Vector2 diff = calculatedPath[calculatedPath.Count - 1] - calculatedPath[calculatedPath.Count - 2];
                double d = diff.Length;

                if (d <= 0)
                    return;

                calculatedPath[calculatedPath.Count - 1] += diff * (float)((Length - l) / d);
                cumulativeLength[calculatedPath.Count - 1] = Length;
            }*/
        }

        public void Calculate()
        {
            calculatePath();
            calculateCumulativeLengthAndTrimPath();
        }

        private int indexOfDistance(double d)
        {
            int i = cumulativeLength.BinarySearch(d);
            if (i < 0) i = ~i;

            return i;
        }

        private double progressToDistance(double progress)
        {
            return MathHelper.Clamp(progress, 0, 1) * Distance;
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
        /// <param name="p0">Start progress. Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        /// <param name="p1">End progress. Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        public void GetPathToProgress(List<Vector2> path, double p0, double p1)
        {
            if (calculatedPath.Count == 0 && ControlPoints.Count > 0)
                Calculate();

            double d0 = progressToDistance(p0);
            double d1 = progressToDistance(p1);

            path.Clear();

            int i = 0;
            for (; i < calculatedPath.Count && cumulativeLength[i] < d0; ++i) { }

            path.Add(interpolateVertices(i, d0) + Offset);

            for (; i < calculatedPath.Count && cumulativeLength[i] <= d1; ++i)
                path.Add(calculatedPath[i] + Offset);

            path.Add(interpolateVertices(i, d1) + Offset);
        }

        /// <summary>
        /// Computes the position on the slider at a given progress that ranges from 0 (beginning of the curve)
        /// to 1 (end of the curve).
        /// </summary>
        /// <param name="progress">Ranges from 0 (beginning of the curve) to 1 (end of the curve).</param>
        /// <returns></returns>
        public Vector2 PositionAt(double progress)
        {
            if (calculatedPath.Count == 0 && ControlPoints.Count > 0)
                Calculate();

            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d) + Offset;
        }
    }
}
