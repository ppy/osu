// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public struct SliderPath : IEquatable<SliderPath>
    {
        /// <summary>
        /// The user-set distance of the path. If non-null, <see cref="Distance"/> will match this value,
        /// and the path will be shortened/lengthened to match this length.
        /// </summary>
        public readonly double? ExpectedDistance;

        /// <summary>
        /// The type of path.
        /// </summary>
        public readonly PathType Type;

        [JsonProperty]
        private Vector2[] controlPoints;

        private List<Vector2> calculatedPath;
        private List<double> cumulativeLength;

        private bool isInitialised;

        /// <summary>
        /// Creates a new <see cref="SliderPath"/>.
        /// </summary>
        /// <param name="type">The type of path.</param>
        /// <param name="controlPoints">The control points of the path.</param>
        /// <param name="expectedDistance">A user-set distance of the path that may be shorter or longer than the true distance between all
        /// <paramref name="controlPoints"/>. The path will be shortened/lengthened to match this length.
        /// If null, the path will use the true distance between all <paramref name="controlPoints"/>.</param>
        [JsonConstructor]
        public SliderPath(PathType type, Vector2[] controlPoints, double? expectedDistance = null)
        {
            this = default;
            this.controlPoints = controlPoints;

            Type = type;
            ExpectedDistance = expectedDistance;

            ensureInitialised();
        }

        /// <summary>
        /// The control points of the path.
        /// </summary>
        [JsonIgnore]
        public ReadOnlySpan<Vector2> ControlPoints
        {
            get
            {
                ensureInitialised();
                return controlPoints.AsSpan();
            }
        }

        /// <summary>
        /// The distance of the path after lengthening/shortening to account for <see cref="ExpectedDistance"/>.
        /// </summary>
        [JsonIgnore]
        public double Distance
        {
            get
            {
                ensureInitialised();
                return cumulativeLength.Count == 0 ? 0 : cumulativeLength[cumulativeLength.Count - 1];
            }
        }

        /// <summary>
        /// Computes the slider path until a given progress that ranges from 0 (beginning of the slider)
        /// to 1 (end of the slider) and stores the generated path in the given list.
        /// </summary>
        /// <param name="path">The list to be filled with the computed path.</param>
        /// <param name="p0">Start progress. Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        /// <param name="p1">End progress. Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        public void GetPathToProgress(List<Vector2> path, double p0, double p1)
        {
            ensureInitialised();

            double d0 = progressToDistance(p0);
            double d1 = progressToDistance(p1);

            path.Clear();

            int i = 0;

            for (; i < calculatedPath.Count && cumulativeLength[i] < d0; ++i)
            {
            }

            path.Add(interpolateVertices(i, d0));

            for (; i < calculatedPath.Count && cumulativeLength[i] <= d1; ++i)
                path.Add(calculatedPath[i]);

            path.Add(interpolateVertices(i, d1));
        }

        /// <summary>
        /// Computes the position on the slider at a given progress that ranges from 0 (beginning of the path)
        /// to 1 (end of the path).
        /// </summary>
        /// <param name="progress">Ranges from 0 (beginning of the path) to 1 (end of the path).</param>
        /// <returns></returns>
        public Vector2 PositionAt(double progress)
        {
            ensureInitialised();

            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d);
        }

        private void ensureInitialised()
        {
            if (isInitialised)
                return;

            isInitialised = true;

            controlPoints = controlPoints ?? Array.Empty<Vector2>();
            calculatedPath = new List<Vector2>();
            cumulativeLength = new List<double>();

            calculatePath();
            calculateCumulativeLength();
        }

        private List<Vector2> calculateSubpath(ReadOnlySpan<Vector2> subControlPoints)
        {
            switch (Type)
            {
                case PathType.Linear:
                    return PathApproximator.ApproximateLinear(subControlPoints);

                case PathType.PerfectCurve:
                    //we can only use CircularArc iff we have exactly three control points and no dissection.
                    if (ControlPoints.Length != 3 || subControlPoints.Length != 3)
                        break;

                    // Here we have exactly 3 control points. Attempt to fit a circular arc.
                    List<Vector2> subpath = PathApproximator.ApproximateCircularArc(subControlPoints);

                    // If for some reason a circular arc could not be fit to the 3 given points, fall back to a numerically stable bezier approximation.
                    if (subpath.Count == 0)
                        break;

                    return subpath;

                case PathType.Catmull:
                    return PathApproximator.ApproximateCatmull(subControlPoints);
            }

            return PathApproximator.ApproximateBezier(subControlPoints);
        }

        private void calculatePath()
        {
            calculatedPath.Clear();

            // Sliders may consist of various subpaths separated by two consecutive vertices
            // with the same position. The following loop parses these subpaths and computes
            // their shape independently, consecutively appending them to calculatedPath.

            int start = 0;
            int end = 0;

            for (int i = 0; i < ControlPoints.Length; ++i)
            {
                end++;

                if (i == ControlPoints.Length - 1 || ControlPoints[i] == ControlPoints[i + 1])
                {
                    ReadOnlySpan<Vector2> cpSpan = ControlPoints.Slice(start, end - start);

                    foreach (Vector2 t in calculateSubpath(cpSpan))
                        if (calculatedPath.Count == 0 || calculatedPath.Last() != t)
                            calculatedPath.Add(t);

                    start = end;
                }
            }
        }

        private void calculateCumulativeLength()
        {
            double l = 0;

            cumulativeLength.Clear();
            cumulativeLength.Add(l);

            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                double d = diff.Length;

                // Shorted slider paths that are too long compared to the expected distance
                if (ExpectedDistance.HasValue && ExpectedDistance - l < d)
                {
                    calculatedPath[i + 1] = calculatedPath[i] + diff * (float)((ExpectedDistance - l) / d);
                    calculatedPath.RemoveRange(i + 2, calculatedPath.Count - 2 - i);

                    l = ExpectedDistance.Value;
                    cumulativeLength.Add(l);
                    break;
                }

                l += d;
                cumulativeLength.Add(l);
            }

            // Lengthen slider paths that are too short compared to the expected distance
            if (ExpectedDistance.HasValue && l < ExpectedDistance && calculatedPath.Count > 1)
            {
                Vector2 diff = calculatedPath[calculatedPath.Count - 1] - calculatedPath[calculatedPath.Count - 2];
                double d = diff.Length;

                if (d <= 0)
                    return;

                calculatedPath[calculatedPath.Count - 1] += diff * (float)((ExpectedDistance - l) / d);
                cumulativeLength[calculatedPath.Count - 1] = ExpectedDistance.Value;
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
            return MathHelper.Clamp(progress, 0, 1) * Distance;
        }

        private Vector2 interpolateVertices(int i, double d)
        {
            if (calculatedPath.Count == 0)
                return Vector2.Zero;

            if (i <= 0)
                return calculatedPath.First();
            if (i >= calculatedPath.Count)
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

        public bool Equals(SliderPath other)
        {
            if (ControlPoints == null && other.ControlPoints != null)
                return false;
            if (other.ControlPoints == null && ControlPoints != null)
                return false;

            return ControlPoints.SequenceEqual(other.ControlPoints) && ExpectedDistance.Equals(other.ExpectedDistance) && Type == other.Type;
        }
    }
}
