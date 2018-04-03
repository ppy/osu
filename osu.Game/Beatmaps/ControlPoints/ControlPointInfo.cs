// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Lists;

namespace osu.Game.Beatmaps.ControlPoints
{
    [Serializable]
    public class ControlPointInfo
    {
        /// <summary>
        /// All timing points.
        /// </summary>
        [JsonProperty]
        public SortedList<TimingControlPoint> TimingPoints { get; private set; } = new SortedList<TimingControlPoint>(Comparer<TimingControlPoint>.Default);

        /// <summary>
        /// All difficulty points.
        /// </summary>
        [JsonProperty]
        public SortedList<DifficultyControlPoint> DifficultyPoints { get; private set; } = new SortedList<DifficultyControlPoint>(Comparer<DifficultyControlPoint>.Default);

        /// <summary>
        /// All sound points.
        /// </summary>
        [JsonProperty]
        public SortedList<SampleControlPoint> SamplePoints { get; private set; } = new SortedList<SampleControlPoint>(Comparer<SampleControlPoint>.Default);

        /// <summary>
        /// All effect points.
        /// </summary>
        [JsonProperty]
        public SortedList<EffectControlPoint> EffectPoints { get; private set; } = new SortedList<EffectControlPoint>(Comparer<EffectControlPoint>.Default);

        /// <summary>
        /// Finds the difficulty control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the difficulty control point at.</param>
        /// <returns>The difficulty control point.</returns>
        public DifficultyControlPoint DifficultyPointAt(double time) => binarySearch(DifficultyPoints, time);

        /// <summary>
        /// Finds the effect control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the effect control point at.</param>
        /// <returns>The effect control point.</returns>
        public EffectControlPoint EffectPointAt(double time) => binarySearch(EffectPoints, time);

        /// <summary>
        /// Finds the sound control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the sound control point at.</param>
        /// <returns>The sound control point.</returns>
        public SampleControlPoint SamplePointAt(double time) => binarySearch(SamplePoints, time, SamplePoints.FirstOrDefault());

        /// <summary>
        /// Finds the timing control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the timing control point at.</param>
        /// <returns>The timing control point.</returns>
        public TimingControlPoint TimingPointAt(double time) => binarySearch(TimingPoints, time, TimingPoints.FirstOrDefault());

        /// <summary>
        /// Finds the maximum BPM represented by any timing control point.
        /// </summary>
        [JsonIgnore]
        public double BPMMaximum =>
            60000 / (TimingPoints.OrderBy(c => c.BeatLength).FirstOrDefault() ?? new TimingControlPoint()).BeatLength;

        /// <summary>
        /// Finds the minimum BPM represented by any timing control point.
        /// </summary>
        [JsonIgnore]
        public double BPMMinimum =>
            60000 / (TimingPoints.OrderByDescending(c => c.BeatLength).FirstOrDefault() ?? new TimingControlPoint()).BeatLength;

        /// <summary>
        /// Finds the mode BPM (most common BPM) represented by the control points.
        /// </summary>
        [JsonIgnore]
        public double BPMMode =>
            60000 / (TimingPoints.GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).FirstOrDefault()?.FirstOrDefault() ?? new TimingControlPoint()).BeatLength;

        /// <summary>
        /// Binary searches one of the control point lists to find the active control point at <paramref name="time"/>.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="time">The time to find the control point at.</param>
        /// <param name="prePoint">The control point to use when <paramref name="time"/> is before any control points. If null, a new control point will be constructed.</param>
        /// <returns>The active control point at <paramref name="time"/>.</returns>
        private T binarySearch<T>(SortedList<T> list, double time, T prePoint = null)
            where T : ControlPoint, new()
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.Count == 0)
                return new T();

            if (time < list[0].Time)
                return prePoint ?? new T();

            int index = list.BinarySearch(new T { Time = time });

            // Check if we've found an exact match (t == time)
            if (index >= 0)
                return list[index];

            index = ~index;

            // BinarySearch will return the index of the first element _greater_ than the search
            // This is the inactive point - the active point is the one before it (index - 1)
            return list[index - 1];
        }
    }
}
