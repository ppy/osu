// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Lists;
using osu.Framework.Utils;
using osu.Game.Screens.Edit;
using osu.Game.Utils;

namespace osu.Game.Beatmaps.ControlPoints
{
    [Serializable]
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class ControlPointInfo : IDeepCloneable<ControlPointInfo>
    {
        /// <summary>
        /// Invoked on any change to the set of control points.
        /// </summary>
        [CanBeNull]
        public event Action ControlPointsChanged;

        private void raiseControlPointsChanged([CanBeNull] ControlPoint _ = null) => ControlPointsChanged?.Invoke();

        /// <summary>
        /// All control points grouped by time.
        /// </summary>
        [JsonProperty]
        public IBindableList<ControlPointGroup> Groups => groups;

        private readonly BindableList<ControlPointGroup> groups = new BindableList<ControlPointGroup>();

        /// <summary>
        /// All timing points.
        /// </summary>
        [JsonProperty]
        public IReadOnlyList<TimingControlPoint> TimingPoints => timingPoints;

        private readonly SortedList<TimingControlPoint> timingPoints = new SortedList<TimingControlPoint>(Comparer<TimingControlPoint>.Default);

        /// <summary>
        /// All effect points.
        /// </summary>
        [JsonProperty]
        public IReadOnlyList<EffectControlPoint> EffectPoints => effectPoints;

        private readonly SortedList<EffectControlPoint> effectPoints = new SortedList<EffectControlPoint>(Comparer<EffectControlPoint>.Default);

        /// <summary>
        /// All control points, of all types.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ControlPoint> AllControlPoints => Groups.SelectMany(g => g.ControlPoints).ToArray();

        /// <summary>
        /// Finds the effect control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the effect control point at.</param>
        /// <returns>The effect control point.</returns>
        [NotNull]
        public EffectControlPoint EffectPointAt(double time) => BinarySearchWithFallback(EffectPoints, time, EffectControlPoint.DEFAULT);

        /// <summary>
        /// Finds the timing control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <returns>The timing control point.</returns>
        [NotNull]
        public TimingControlPoint TimingPointAt(double time) => BinarySearchWithFallback(TimingPoints, time, TimingPoints.Count > 0 ? TimingPoints[0] : TimingControlPoint.DEFAULT);

        /// <summary>
        /// Finds the first timing point that is active strictly after <paramref name="time"/>, or null if no such point exists.
        /// </summary>
        /// <param name="time">The time after which to find the timing control point.</param>
        /// <returns>The timing control point.</returns>
        [CanBeNull]
        public TimingControlPoint TimingPointAfter(double time)
        {
            int index = BinarySearch(TimingPoints, time, EqualitySelection.Rightmost);
            index = index < 0 ? ~index : index + 1;
            return index < TimingPoints.Count ? TimingPoints[index] : null;
        }

        /// <summary>
        /// Finds the maximum BPM represented by any timing control point.
        /// </summary>
        [JsonIgnore]
        public double BPMMaximum =>
            60000 / (TimingPoints.MinBy(c => c.BeatLength) ?? TimingControlPoint.DEFAULT).BeatLength;

        /// <summary>
        /// Finds the minimum BPM represented by any timing control point.
        /// </summary>
        [JsonIgnore]
        public double BPMMinimum =>
            60000 / (TimingPoints.MaxBy(c => c.BeatLength) ?? TimingControlPoint.DEFAULT).BeatLength;

        /// <summary>
        /// Remove all <see cref="ControlPointGroup"/>s and return to a pristine state.
        /// </summary>
        public virtual void Clear()
        {
            groups.Clear();
            timingPoints.Clear();
            effectPoints.Clear();
        }

        /// <summary>
        /// Add a new <see cref="ControlPoint"/>. Note that the provided control point may not be added if the correct state is already present at the provided time.
        /// </summary>
        /// <param name="time">The time at which the control point should be added.</param>
        /// <param name="controlPoint">The control point to add.</param>
        /// <returns>Whether the control point was added.</returns>
        public bool Add(double time, ControlPoint controlPoint)
        {
            if (CheckAlreadyExisting(time, controlPoint))
                return false;

            GroupAt(time, true).Add(controlPoint);
            return true;
        }

        public ControlPointGroup GroupAt(double time, bool addIfNotExisting = false)
        {
            var newGroup = new ControlPointGroup(time);

            int i = groups.BinarySearch(newGroup);

            if (i >= 0)
                return groups[i];

            if (addIfNotExisting)
            {
                newGroup.ItemAdded += GroupItemAdded;
                newGroup.ItemChanged += raiseControlPointsChanged;
                newGroup.ItemRemoved += GroupItemRemoved;

                groups.Insert(~i, newGroup);
                return newGroup;
            }

            return null;
        }

        public void RemoveGroup(ControlPointGroup group)
        {
            foreach (var item in group.ControlPoints.ToArray())
                group.Remove(item);

            group.ItemAdded -= GroupItemAdded;
            group.ItemChanged -= raiseControlPointsChanged;
            group.ItemRemoved -= GroupItemRemoved;

            groups.Remove(group);
        }

        /// <summary>
        /// Returns the time on the given beat divisor closest to the given time.
        /// </summary>
        /// <param name="time">The time to find the closest snapped time to.</param>
        /// <param name="beatDivisor">The beat divisor to snap to.</param>
        /// <param name="referenceTime">An optional reference point to use for timing point lookup.</param>
        public double GetClosestSnappedTime(double time, int beatDivisor, double? referenceTime = null)
        {
            var timingPoint = TimingPointAt(referenceTime ?? time);
            double snappedTime = getClosestSnappedTime(timingPoint, time, beatDivisor);

            if (referenceTime.HasValue)
                return snappedTime;

            // If there is a timing point right after the given time, we should check if it is closer than the snapped time and snap to it.
            var timingPointAfter = TimingPointAfter(time);
            return timingPointAfter is null || Math.Abs(time - snappedTime) < Math.Abs(time - timingPointAfter.Time) ? snappedTime : timingPointAfter.Time;
        }

        /// <summary>
        /// Returns the time on *ANY* valid beat divisor, favouring the divisor closest to the given time.
        /// </summary>
        /// <param name="time">The time to find the closest snapped time to.</param>
        public double GetClosestSnappedTime(double time) => GetClosestSnappedTime(time, GetClosestBeatDivisor(time));

        /// <summary>
        /// Returns the beat snap divisor closest to the given time. If two are equally close, the smallest divisor is returned.
        /// </summary>
        /// <param name="time">The time to find the closest beat snap divisor to.</param>
        /// <param name="referenceTime">An optional reference point to use for timing point lookup.</param>
        public int GetClosestBeatDivisor(double time, double? referenceTime = null)
        {
            TimingControlPoint timingPoint = TimingPointAt(referenceTime ?? time);

            int closestDivisor = 0;
            double closestTime = double.MaxValue;

            foreach (int divisor in BindableBeatDivisor.PREDEFINED_DIVISORS)
            {
                double distanceFromSnap = Math.Abs(time - getClosestSnappedTime(timingPoint, time, divisor));

                if (Precision.DefinitelyBigger(closestTime, distanceFromSnap))
                {
                    closestDivisor = divisor;
                    closestTime = distanceFromSnap;
                }
            }

            return closestDivisor;
        }

        private static double getClosestSnappedTime(TimingControlPoint timingPoint, double time, int beatDivisor)
        {
            double beatLength = timingPoint.BeatLength / beatDivisor;
            double beats = (Math.Max(time, 0) - timingPoint.Time) / beatLength;

            int roundedBeats = (int)Math.Round(beats, MidpointRounding.AwayFromZero);
            double snappedTime = timingPoint.Time + roundedBeats * beatLength;

            if (snappedTime >= 0)
                return snappedTime;

            return snappedTime + beatLength;
        }

        /// <summary>
        /// Binary searches one of the control point lists to find the active control point at <paramref name="time"/>.
        /// Includes logic for returning a specific point when no matching point is found.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="time">The time to find the control point at.</param>
        /// <param name="fallback">The control point to use when <paramref name="time"/> is before any control points.</param>
        /// <returns>The active control point at <paramref name="time"/>, or a fallback <see cref="ControlPoint"/> if none found.</returns>
        public static T BinarySearchWithFallback<T>(IReadOnlyList<T> list, double time, T fallback)
            where T : class, IControlPoint
        {
            return BinarySearch(list, time) ?? fallback;
        }

        /// <summary>
        /// Binary searches one of the control point lists to find the active control point at <paramref name="time"/>.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="time">The time to find the control point at.</param>
        /// <returns>The active control point at <paramref name="time"/>. Will return <c>null</c> if there are no control points, or if the time is before the first control point.</returns>
        public static T BinarySearch<T>(IReadOnlyList<T> list, double time)
            where T : class, IControlPoint
        {
            ArgumentNullException.ThrowIfNull(list);

            int index = BinarySearch(list, time, EqualitySelection.Rightmost);

            if (index < 0)
                index = ~index - 1;

            return index >= 0 ? list[index] : null;
        }

        /// <summary>
        /// Binary searches one of the control point lists to find the active control point at <paramref name="time"/>.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="time">The time to find the control point at.</param>
        /// <param name="equalitySelection">Determines which index to return if there are multiple exact matches.</param>
        /// <returns>The index of the control point at <paramref name="time"/>. Will return the complement of the index of the control point after <paramref name="time"/> if no exact match is found.</returns>
        public static int BinarySearch<T>(IReadOnlyList<T> list, double time, EqualitySelection equalitySelection)
            where T : class, IControlPoint
        {
            ArgumentNullException.ThrowIfNull(list);

            int n = list.Count;

            if (n == 0)
                return -1;

            if (time < list[0].Time)
                return -1;

            if (time > list[^1].Time)
                return ~n;

            int l = 0;
            int r = n - 1;
            bool equalityFound = false;

            while (l <= r)
            {
                int pivot = l + ((r - l) >> 1);

                if (list[pivot].Time < time)
                    l = pivot + 1;
                else if (list[pivot].Time > time)
                    r = pivot - 1;
                else
                {
                    equalityFound = true;

                    switch (equalitySelection)
                    {
                        case EqualitySelection.Leftmost:
                            r = pivot - 1;
                            break;

                        case EqualitySelection.Rightmost:
                            l = pivot + 1;
                            break;

                        default:
                        case EqualitySelection.FirstFound:
                            return pivot;
                    }
                }
            }

            if (!equalityFound) return ~l;

            switch (equalitySelection)
            {
                case EqualitySelection.Leftmost:
                    return l;

                default:
                case EqualitySelection.Rightmost:
                    return l - 1;
            }
        }

        /// <summary>
        /// Check whether <paramref name="newPoint"/> should be added.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <param name="newPoint">A point to be added.</param>
        /// <returns>Whether the new point should be added.</returns>
        protected virtual bool CheckAlreadyExisting(double time, ControlPoint newPoint)
        {
            ControlPoint existing = null;

            switch (newPoint)
            {
                case TimingControlPoint:
                    // Timing points are a special case and need to be added regardless of fallback availability.
                    existing = BinarySearch(TimingPoints, time);
                    break;

                case EffectControlPoint:
                    existing = EffectPointAt(time);
                    break;
            }

            return newPoint?.IsRedundant(existing) == true;
        }

        protected virtual void GroupItemAdded(ControlPoint controlPoint)
        {
            switch (controlPoint)
            {
                case TimingControlPoint typed:
                    timingPoints.Add(typed);
                    break;

                case EffectControlPoint typed:
                    effectPoints.Add(typed);
                    break;

                default:
                    throw new ArgumentException($"A control point of unexpected type {controlPoint.GetType()} was added to this {nameof(ControlPointInfo)}");
            }

            raiseControlPointsChanged();
        }

        protected virtual void GroupItemRemoved(ControlPoint controlPoint)
        {
            switch (controlPoint)
            {
                case TimingControlPoint typed:
                    timingPoints.Remove(typed);
                    break;

                case EffectControlPoint typed:
                    effectPoints.Remove(typed);
                    break;
            }

            raiseControlPointsChanged();
        }

        public ControlPointInfo DeepClone()
        {
            var controlPointInfo = (ControlPointInfo)Activator.CreateInstance(GetType())!;

            foreach (var point in AllControlPoints)
                controlPointInfo.Add(point.Time, point.DeepClone());

            return controlPointInfo;
        }
    }

    public enum EqualitySelection
    {
        FirstFound,
        Leftmost,
        Rightmost
    }
}
