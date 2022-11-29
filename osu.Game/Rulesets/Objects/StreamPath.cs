// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class StreamPath
    {
        /// <summary>
        /// The current version of this <see cref="StreamPath"/>. Updated when any change to the path occurs.
        /// </summary>
        [JsonIgnore]
        public IBindable<int> Version => version;

        private readonly Bindable<int> version = new Bindable<int>();

        private readonly List<(Vector2, double)> calculatedPath = new List<(Vector2, double)>();
        private readonly Cached pathCache = new Cached();

        private readonly SliderPath path = new SliderPath();

        public SliderPath Path
        {
            get => path;
            set
            {
                path.ControlPoints.Clear();
                path.ExpectedDistance.Value = null;
                path.ControlPoints.AddRange(value.ControlPoints.Select(c => new PathControlPoint(c.Position, c.Type)));
            }
        }

        /// <summary>
        /// The control points of the path.
        /// </summary>
        public readonly BindableList<StreamControlPoint> ControlPoints = new BindableList<StreamControlPoint>();

        /// <summary>
        /// Creates a new <see cref="SliderPath"/>.
        /// </summary>
        public StreamPath()
        {
            Path.Version.ValueChanged += _ => invalidate();

            ControlPoints.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var c in args.NewItems.Cast<StreamControlPoint>())
                            c.Changed += invalidate;
                        break;

                    case NotifyCollectionChangedAction.Reset:
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var c in args.OldItems.Cast<StreamControlPoint>())
                            c.Changed -= invalidate;
                        break;
                }

                invalidate();
            };
        }

        /// <summary>
        /// Creates a new <see cref="StreamPath"/> initialised with a list of control points.
        /// </summary>
        /// <param name="pathControlPoints">An optional set of <see cref="PathControlPoint"/>s to initialise <see cref="Path"/> with.</param>
        /// <param name="streamControlPoints">An optional set of <see cref="StreamControlPoint"/>s to initialise <see cref="ControlPoints"/> with.</param>
        [JsonConstructor]
        public StreamPath(PathControlPoint[] pathControlPoints, StreamControlPoint[] streamControlPoints)
            : this()
        {
            Path.ControlPoints.AddRange(pathControlPoints);
            ControlPoints.AddRange(streamControlPoints);
        }

        public List<(Vector2, double)> GetStreamPath()
        {
            ensureValid();

            return calculatedPath.ToList();
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

            pathCache.Validate();
        }

        private void calculatePath()
        {
            calculatedPath.Clear();

            if (Path.ControlPoints.Count == 1)
            {
                calculatedPath.Add((Path.ControlPoints[0].Position, 0));
                return;
            }

            double[] segments = Path.GetSegmentEnds().ToArray();

            for (int i = 0; i < Math.Min(segments.Length - 1, ControlPoints.Count - 1); i++)
            {
                var streamControlPoint = ControlPoints[i];
                var nextStreamControlPoint = ControlPoints[i + 1];
                double duration = nextStreamControlPoint.Time - streamControlPoint.Time;
                double accel = nextStreamControlPoint.Acceleration;
                int count = Math.Max(nextStreamControlPoint.Count, 1);

                double segmentStart = segments[i];
                double segmentEnd = segments[i + 1];
                double segmentLength = segmentEnd - segmentStart;

                if (nextStreamControlPoint.Exponential)
                {
                    // The distance between each stream point multiplies by Ratio every iteration
                    // Make sure that: sum 0 <= i < count: Ratio^i * dist == segmentLength
                    double ratio = Math.Pow(2, accel * 0.1d);
                    double dist = segmentLength / ratioSum(ratio, count);
                    double t = segmentStart;

                    for (int j = 0; j < count; j++)
                    {
                        calculatedPath.Add((Path.PositionAt(t), streamControlPoint.Time + duration * j / count));
                        t += dist;
                        dist *= ratio;
                    }
                }
                else
                {
                    // The interpolation is quadratic
                    for (int j = 0; j < count; j++)
                    {
                        // Use L'Hôpital's rule to find the limit in the case where ratio == -1
                        double x = (double)j / count;
                        double t = Precision.AlmostEquals(accel, -1) ? Math.Log(x + 1) / Math.Log(2) : (Math.Pow(x + 1, accel + 1) - 1) / (Math.Pow(2, accel + 1) - 1);
                        calculatedPath.Add((Path.PositionAt(segmentStart + segmentLength * t), streamControlPoint.Time + duration * j / count));
                    }
                }
            }

            if (ControlPoints.Count > 1)
                calculatedPath.Add((Path.PositionAt(1), ControlPoints.Last().Time));
        }

        private double ratioSum(double ratio, int count)
        {
            double mult = 1;
            double result = 0;

            for (int i = 0; i < count; i++)
            {
                result += mult;
                mult *= ratio;
            }

            return result;
        }
    }
}
