// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class SliderPath
    {
        /// <summary>
        /// The current version of this <see cref="SliderPath"/>. Updated when any change to the path occurs.
        /// </summary>
        [JsonIgnore]
        public IBindable<int> Version => version;

        private readonly Bindable<int> version = new Bindable<int>();

        /// <summary>
        /// The user-set distance of the path. If non-null, <see cref="Distance"/> will match this value,
        /// and the path will be shortened/lengthened to match this length.
        /// </summary>
        public readonly Bindable<double?> ExpectedDistance = new Bindable<double?>();

        public bool HasValidLength => Distance > 0;

        /// <summary>
        /// The control points of the path.
        /// </summary>
        public readonly BindableList<PathControlPoint> ControlPoints = new BindableList<PathControlPoint>();

        private readonly List<Vector2> calculatedPath = new List<Vector2>();
        private readonly List<double> cumulativeLength = new List<double>();
        private readonly Cached pathCache = new Cached();

        private double calculatedLength;

        private readonly List<int> segmentEnds = new List<int>();
        private double[] segmentEndDistances = Array.Empty<double>();

        /// <summary>
        /// Creates a new <see cref="SliderPath"/>.
        /// </summary>
        public SliderPath()
        {
            ExpectedDistance.ValueChanged += _ => invalidate();

            ControlPoints.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(args.NewItems != null);

                        foreach (var c in args.NewItems.Cast<PathControlPoint>())
                            c.Changed += invalidate;
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(args.OldItems != null);

                        foreach (var c in args.OldItems.Cast<PathControlPoint>())
                            c.Changed -= invalidate;
                        break;
                }

                invalidate();
            };
        }

        /// <summary>
        /// Creates a new <see cref="SliderPath"/> initialised with a list of control points.
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
            : this(controlPoints.Select((c, i) => new PathControlPoint(c, i == 0 ? type : null)).ToArray(), expectedDistance)
        {
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
                return cumulativeLength.Count == 0 ? 0 : cumulativeLength[^1];
            }
        }

        /// <summary>
        /// The distance of the path prior to lengthening/shortening to account for <see cref="ExpectedDistance"/>.
        /// </summary>
        public double CalculatedDistance
        {
            get
            {
                ensureValid();
                return calculatedLength;
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
        public Vector2 PositionAt(double progress)
        {
            ensureValid();

            double d = progressToDistance(progress);
            return interpolateVertices(indexOfDistance(d), d);
        }

        /// <summary>
        /// Returns the control points belonging to the same segment as the one given.
        /// The first point has a PathType which all other points inherit.
        /// </summary>
        /// <param name="controlPoint">One of the control points in the segment.</param>
        public List<PathControlPoint> PointsInSegment(PathControlPoint controlPoint)
        {
            bool found = false;
            List<PathControlPoint> pointsInCurrentSegment = new List<PathControlPoint>();

            foreach (PathControlPoint point in ControlPoints)
            {
                if (point.Type != null)
                {
                    if (!found)
                        pointsInCurrentSegment.Clear();
                    else
                    {
                        pointsInCurrentSegment.Add(point);
                        break;
                    }
                }

                pointsInCurrentSegment.Add(point);

                if (point == controlPoint)
                    found = true;
            }

            return pointsInCurrentSegment;
        }

        /// <summary>
        /// Returns the progress values at which (control point) segments of the path end.
        /// Ranges from 0 (beginning of the path) to 1 (end of the path) to infinity (beyond the end of the path).
        /// </summary>
        /// <remarks>
        /// <see cref="PositionAt"/> truncates the progression values to [0,1],
        /// so you can't use this method in conjunction with that one to retrieve the positions of segment ends beyond the end of the path.
        /// </remarks>
        /// <example>
        /// <para>
        /// In case <see cref="Distance"/> is less than <see cref="CalculatedDistance"/>,
        /// the last segment ends after the end of the path, hence it returns a value greater than 1.
        /// </para>
        /// <para>
        /// In case <see cref="Distance"/> is greater than <see cref="CalculatedDistance"/>,
        /// the last segment ends before the end of the path, hence it returns a value less than 1.
        /// </para>
        /// </example>
        public IEnumerable<double> GetSegmentEnds()
        {
            ensureValid();

            return segmentEndDistances.Select(d => d / Distance);
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
            calculateLength();

            pathCache.Validate();
        }

        private void calculatePath()
        {
            calculatedPath.Clear();
            segmentEnds.Clear();

            if (ControlPoints.Count == 0)
                return;

            Vector2[] vertices = new Vector2[ControlPoints.Count];
            for (int i = 0; i < ControlPoints.Count; i++)
                vertices[i] = ControlPoints[i].Position;

            int start = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].Type == null && i < ControlPoints.Count - 1)
                    continue;

                // The current vertex ends the segment
                var segmentVertices = vertices.AsSpan().Slice(start, i - start + 1);
                var segmentType = ControlPoints[start].Type ?? PathType.LINEAR;

                // No need to calculate path when there is only 1 vertex
                if (segmentVertices.Length == 1)
                    calculatedPath.Add(segmentVertices[0]);
                else if (segmentVertices.Length > 1)
                {
                    List<Vector2> subPath = calculateSubPath(segmentVertices, segmentType);
                    // Skip the first vertex if it is the same as the last vertex from the previous segment
                    int skipFirst = calculatedPath.Count > 0 && subPath.Count > 0 && calculatedPath.Last() == subPath[0] ? 1 : 0;

                    foreach (Vector2 t in subPath.Skip(skipFirst))
                        calculatedPath.Add(t);
                }

                if (i > 0)
                {
                    // Remember the index of the segment end
                    segmentEnds.Add(calculatedPath.Count - 1);
                }

                // Start the new segment at the current vertex
                start = i;
            }
        }

        private List<Vector2> calculateSubPath(ReadOnlySpan<Vector2> subControlPoints, PathType type)
        {
            switch (type.SplineType)
            {
                case SplineType.Linear:
                    return PathApproximator.ApproximateLinear(subControlPoints);

                case SplineType.PerfectCurve:
                    if (subControlPoints.Length != 3)
                        break;

                    List<Vector2> subPath = PathApproximator.ApproximateCircularArc(subControlPoints);

                    // If for some reason a circular arc could not be fit to the 3 given points, fall back to a numerically stable bezier approximation.
                    if (subPath.Count == 0)
                        break;

                    return subPath;

                case SplineType.Catmull:
                    return PathApproximator.ApproximateCatmull(subControlPoints);
            }

            return PathApproximator.ApproximateBSpline(subControlPoints, type.Degree ?? subControlPoints.Length);
        }

        private void calculateLength()
        {
            calculatedLength = 0;
            cumulativeLength.Clear();
            cumulativeLength.Add(0);

            for (int i = 0; i < calculatedPath.Count - 1; i++)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                calculatedLength += diff.Length;
                cumulativeLength.Add(calculatedLength);
            }

            // Store the distances of the segment ends now, because after shortening the indices may be out of range
            segmentEndDistances = new double[segmentEnds.Count];

            for (int i = 0; i < segmentEnds.Count; i++)
            {
                segmentEndDistances[i] = cumulativeLength[segmentEnds[i]];
            }

            if (ExpectedDistance.Value is double expectedDistance && calculatedLength != expectedDistance)
            {
                // In osu-stable, if the last two path points of a slider are equal, extension is not performed.
                if (calculatedPath.Count >= 2 && calculatedPath[^1] == calculatedPath[^2] && expectedDistance > calculatedLength)
                {
                    cumulativeLength.Add(calculatedLength);
                    return;
                }

                // The last length is always incorrect
                cumulativeLength.RemoveAt(cumulativeLength.Count - 1);

                int pathEndIndex = calculatedPath.Count - 1;

                if (calculatedLength > expectedDistance)
                {
                    // The path will be shortened further, in which case we should trim any more unnecessary lengths and their associated path segments
                    while (cumulativeLength.Count > 0 && cumulativeLength[^1] >= expectedDistance)
                    {
                        cumulativeLength.RemoveAt(cumulativeLength.Count - 1);
                        calculatedPath.RemoveAt(pathEndIndex--);
                    }
                }

                if (pathEndIndex <= 0)
                {
                    // The expected distance is negative or zero
                    // TODO: Perhaps negative path lengths should be disallowed altogether
                    cumulativeLength.Add(0);
                    return;
                }

                // The direction of the segment to shorten or lengthen
                Vector2 dir = (calculatedPath[pathEndIndex] - calculatedPath[pathEndIndex - 1]).Normalized();

                calculatedPath[pathEndIndex] = calculatedPath[pathEndIndex - 1] + dir * (float)(expectedDistance - cumulativeLength[^1]);
                cumulativeLength.Add(expectedDistance);
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
