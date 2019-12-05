// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class SliderPath
    {
        /// <summary>
        /// The current version of this <see cref="SliderPath"/>. Updated when any change to the path occurs.
        /// </summary>
        public IBindable<int> Version => version;

        private readonly Bindable<int> version = new Bindable<int>();

        /// <summary>
        /// The user-set distance of the path. If non-null, <see cref="Distance"/> will match this value,
        /// and the path will be shortened/lengthened to match this length.
        /// </summary>
        public readonly Bindable<double?> ExpectedDistance = new Bindable<double?>();

        /// <summary>
        /// The control points of the path.
        /// </summary>
        public readonly BindableList<PathControlPoint> ControlPoints = new BindableList<PathControlPoint>();

        public readonly List<int> Test = new List<int>();

        private readonly Cached pathCache = new Cached();

        private readonly List<Vector2> calculatedPath = new List<Vector2>();
        private readonly List<double> cumulativeLength = new List<double>();

        /// <summary>
        /// Creates a new <see cref="SliderPath"/>.
        /// </summary>
        public SliderPath()
        {
            ExpectedDistance.ValueChanged += _ => invalidate();

            ControlPoints.ItemsAdded += items =>
            {
                foreach (var c in items)
                    c.Changed += invalidate;

                invalidate();
            };

            ControlPoints.ItemsRemoved += items =>
            {
                foreach (var c in items)
                    c.Changed -= invalidate;

                invalidate();
            };
        }

        /// <summary>
        /// Creates a new <see cref="SliderPath"/>.
        /// </summary>
        /// <param name="controlPoints">An optional set of <see cref="PathControlPoint"/>s to initialise the path with.</param>
        /// <param name="expectedDistance">A user-set distance of the path that may be shorter or longer than the true distance between all control points.
        /// The path will be shortened/lengthened to match this length. If null, the path will use the true distance between all control points.</param>
        [JsonConstructor]
        public SliderPath(PathControlPoint[] controlPoints, double? expectedDistance = null)
            : this()
        {
            ControlPoints.AddRange(controlPoints);
            ExpectedDistance.Value = expectedDistance;
        }

        public SliderPath(PathType type, Vector2[] controlPoints, double? expectedDistance = null)
            : this()
        {
            foreach (var c in controlPoints)
                ControlPoints.Add(new PathControlPoint { Position = { Value = c } });
            ControlPoints[0].Type.Value = type;

            ExpectedDistance.Value = expectedDistance;
        }

        /// <summary>
        /// The distance of the path after lengthening/shortening to account for <see cref="ExpectedDistance"/>.
        /// </summary>
        [JsonIgnore]
        public double Distance
        {
            get
            {
                ensureValid();
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
            ensureValid();

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
            ensureValid();

            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d);
        }

        private void invalidate()
        {
            pathCache.Invalidate();
            version.Value++;
        }

        private void ensureValid()
        {
            if (pathCache.IsValid)
                return;

            calculatePath();
            calculateCumulativeLength();

            pathCache.Validate();
        }

        private void calculatePath()
        {
            calculatedPath.Clear();

            if (ControlPoints.Count == 0)
                return;

            if (ControlPoints[0].Type.Value == null)
                throw new InvalidOperationException($"The first control point in a {nameof(SliderPath)} must have a non-null type.");

            Vector2[] vertices = new Vector2[ControlPoints.Count];
            for (int i = 0; i < ControlPoints.Count; i++)
                vertices[i] = ControlPoints[i].Position.Value;

            int start = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].Type.Value == null && i < ControlPoints.Count - 1)
                    continue;

                Debug.Assert(ControlPoints[start].Type.Value.HasValue);

                // The current vertex ends the segment
                var segmentVertices = vertices.AsSpan().Slice(start, i - start + 1);
                var segmentType = ControlPoints[start].Type.Value.Value;

                foreach (Vector2 t in computeSubPath(segmentVertices, segmentType))
                {
                    if (calculatedPath.Count == 0 || calculatedPath.Last() != t)
                        calculatedPath.Add(t);
                }

                // Start the new segment at the current vertex
                start = i;
            }

            static List<Vector2> computeSubPath(ReadOnlySpan<Vector2> subControlPoints, PathType type)
            {
                switch (type)
                {
                    case PathType.Linear:
                        return PathApproximator.ApproximateLinear(subControlPoints);

                    case PathType.PerfectCurve:
                        if (subControlPoints.Length != 3)
                            break;

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
        }

        private void calculateCumulativeLength()
        {
            double l = 0;

            cumulativeLength.Clear();
            cumulativeLength.Add(l);

            double? expectedDistance = ExpectedDistance.Value;

            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                double d = diff.Length;

                // Shorted slider paths that are too long compared to the expected distance
                if (expectedDistance.HasValue && expectedDistance - l < d)
                {
                    calculatedPath[i + 1] = calculatedPath[i] + diff * (float)((expectedDistance - l) / d);
                    calculatedPath.RemoveRange(i + 2, calculatedPath.Count - 2 - i);

                    l = expectedDistance.Value;
                    cumulativeLength.Add(l);
                    break;
                }

                l += d;
                cumulativeLength.Add(l);
            }

            // Lengthen slider paths that are too short compared to the expected distance
            if (expectedDistance.HasValue && l < expectedDistance && calculatedPath.Count > 1)
            {
                Vector2 diff = calculatedPath[calculatedPath.Count - 1] - calculatedPath[calculatedPath.Count - 2];
                double d = diff.Length;

                if (d <= 0)
                    return;

                calculatedPath[calculatedPath.Count - 1] += diff * (float)((expectedDistance - l) / d);
                cumulativeLength[calculatedPath.Count - 1] = expectedDistance.Value;
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
            return Math.Clamp(progress, 0, 1) * Distance;
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
    }
}
