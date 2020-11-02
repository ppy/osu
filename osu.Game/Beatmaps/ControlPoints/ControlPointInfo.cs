// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Lists;

namespace osu.Game.Beatmaps.ControlPoints
{
    [Serializable]
    public class ControlPointInfo
    {
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
        /// All difficulty points.
        /// </summary>
        [JsonProperty]
        public IReadOnlyList<DifficultyControlPoint> DifficultyPoints => difficultyPoints;

        private readonly SortedList<DifficultyControlPoint> difficultyPoints = new SortedList<DifficultyControlPoint>(Comparer<DifficultyControlPoint>.Default);

        /// <summary>
        /// All sound points.
        /// </summary>
        [JsonProperty]
        public IBindableList<SampleControlPoint> SamplePoints => samplePoints;

        private readonly BindableList<SampleControlPoint> samplePoints = new BindableList<SampleControlPoint>();

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
        /// Finds the difficulty control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the difficulty control point at.</param>
        /// <returns>The difficulty control point.</returns>
        public DifficultyControlPoint DifficultyPointAt(double time) => binarySearchWithFallback(DifficultyPoints, time, DifficultyControlPoint.DEFAULT);

        /// <summary>
        /// Finds the effect control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the effect control point at.</param>
        /// <returns>The effect control point.</returns>
        public EffectControlPoint EffectPointAt(double time) => binarySearchWithFallback(EffectPoints, time, EffectControlPoint.DEFAULT);

        /// <summary>
        /// Finds the sound control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the sound control point at.</param>
        /// <returns>The sound control point.</returns>
        public SampleControlPoint SamplePointAt(double time) => binarySearchWithFallback(SamplePoints, time, SamplePoints.Count > 0 ? SamplePoints[0] : SampleControlPoint.DEFAULT);

        /// <summary>
        /// Finds the timing control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <returns>The timing control point.</returns>
        public TimingControlPoint TimingPointAt(double time) => binarySearchWithFallback(TimingPoints, time, TimingPoints.Count > 0 ? TimingPoints[0] : TimingControlPoint.DEFAULT);

        /// <summary>
        /// Finds the maximum BPM represented by any timing control point.
        /// </summary>
        [JsonIgnore]
        public double BPMMaximum =>
            60000 / (TimingPoints.OrderBy(c => c.BeatLength).FirstOrDefault() ?? TimingControlPoint.DEFAULT).BeatLength;

        /// <summary>
        /// Finds the minimum BPM represented by any timing control point.
        /// </summary>
        [JsonIgnore]
        public double BPMMinimum =>
            60000 / (TimingPoints.OrderByDescending(c => c.BeatLength).FirstOrDefault() ?? TimingControlPoint.DEFAULT).BeatLength;

        /// <summary>
        /// Finds the mode BPM (most common BPM) represented by the control points.
        /// </summary>
        [JsonIgnore]
        public double BPMMode =>
            60000 / (TimingPoints.GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).FirstOrDefault()?.FirstOrDefault() ?? TimingControlPoint.DEFAULT).BeatLength;

        /// <summary>
        /// Remove all <see cref="ControlPointGroup"/>s and return to a pristine state.
        /// </summary>
        public void Clear()
        {
            groups.Clear();
            timingPoints.Clear();
            difficultyPoints.Clear();
            samplePoints.Clear();
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
            if (checkAlreadyExisting(time, controlPoint))
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
                newGroup.ItemAdded += groupItemAdded;
                newGroup.ItemRemoved += groupItemRemoved;

                groups.Insert(~i, newGroup);
                return newGroup;
            }

            return null;
        }

        public void RemoveGroup(ControlPointGroup group)
        {
            foreach (var item in group.ControlPoints.ToArray())
                group.Remove(item);

            group.ItemAdded -= groupItemAdded;
            group.ItemRemoved -= groupItemRemoved;

            groups.Remove(group);
        }

        /// <summary>
        /// Binary searches one of the control point lists to find the active control point at <paramref name="time"/>.
        /// Includes logic for returning a specific point when no matching point is found.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="time">The time to find the control point at.</param>
        /// <param name="fallback">The control point to use when <paramref name="time"/> is before any control points.</param>
        /// <returns>The active control point at <paramref name="time"/>, or a fallback <see cref="ControlPoint"/> if none found.</returns>
        private T binarySearchWithFallback<T>(IReadOnlyList<T> list, double time, T fallback)
            where T : ControlPoint
        {
            return binarySearch(list, time) ?? fallback;
        }

        /// <summary>
        /// Binary searches one of the control point lists to find the active control point at <paramref name="time"/>.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="time">The time to find the control point at.</param>
        /// <returns>The active control point at <paramref name="time"/>.</returns>
        private T binarySearch<T>(IReadOnlyList<T> list, double time)
            where T : ControlPoint
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.Count == 0)
                return null;

            if (time < list[0].Time)
                return null;

            if (time >= list[^1].Time)
                return list[^1];

            int l = 0;
            int r = list.Count - 2;

            while (l <= r)
            {
                int pivot = l + ((r - l) >> 1);

                if (list[pivot].Time < time)
                    l = pivot + 1;
                else if (list[pivot].Time > time)
                    r = pivot - 1;
                else
                    return list[pivot];
            }

            // l will be the first control point with Time > time, but we want the one before it
            return list[l - 1];
        }

        /// <summary>
        /// Check whether <paramref name="newPoint"/> should be added.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <param name="newPoint">A point to be added.</param>
        /// <returns>Whether the new point should be added.</returns>
        private bool checkAlreadyExisting(double time, ControlPoint newPoint)
        {
            ControlPoint existing = null;

            switch (newPoint)
            {
                case TimingControlPoint _:
                    // Timing points are a special case and need to be added regardless of fallback availability.
                    existing = binarySearch(TimingPoints, time);
                    break;

                case EffectControlPoint _:
                    existing = EffectPointAt(time);
                    break;

                case SampleControlPoint _:
                    existing = binarySearch(SamplePoints, time);
                    break;

                case DifficultyControlPoint _:
                    existing = DifficultyPointAt(time);
                    break;
            }

            return newPoint?.IsRedundant(existing) == true;
        }

        private void groupItemAdded(ControlPoint controlPoint)
        {
            switch (controlPoint)
            {
                case TimingControlPoint typed:
                    timingPoints.Add(typed);
                    break;

                case EffectControlPoint typed:
                    effectPoints.Add(typed);
                    break;

                case SampleControlPoint typed:
                    samplePoints.Add(typed);
                    break;

                case DifficultyControlPoint typed:
                    difficultyPoints.Add(typed);
                    break;
            }
        }

        private void groupItemRemoved(ControlPoint controlPoint)
        {
            switch (controlPoint)
            {
                case TimingControlPoint typed:
                    timingPoints.Remove(typed);
                    break;

                case EffectControlPoint typed:
                    effectPoints.Remove(typed);
                    break;

                case SampleControlPoint typed:
                    samplePoints.Remove(typed);
                    break;

                case DifficultyControlPoint typed:
                    difficultyPoints.Remove(typed);
                    break;
            }
        }
    }
}
