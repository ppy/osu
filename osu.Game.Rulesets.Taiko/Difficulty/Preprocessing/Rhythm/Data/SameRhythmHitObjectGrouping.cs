// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Represents a group of <see cref="TaikoDifficultyHitObject"/>s with no rhythm variation.
    /// </summary>
    public class SameRhythmHitObjectGrouping : IHasInterval
    {
        public readonly List<TaikoDifficultyHitObject> HitObjects;

        public TaikoDifficultyHitObject FirstHitObject => HitObjects[0];

        public readonly SameRhythmHitObjectGrouping? Previous;

        /// <summary>
        /// <see cref="DifficultyHitObject.StartTime"/> of the first hit object.
        /// </summary>
        public double StartTime => HitObjects[0].StartTime;

        /// <summary>
        /// The interval between the first and final hit object within this group.
        /// </summary>
        public double Duration => HitObjects[^1].StartTime - HitObjects[0].StartTime;

        /// <summary>
        /// The interval in ms of each hit object in this <see cref="SameRhythmHitObjectGrouping"/>. This is only defined if there is
        /// more than two hit objects in this <see cref="SameRhythmHitObjectGrouping"/>.
        /// </summary>
        public readonly double? HitObjectInterval;

        /// <summary>
        /// The ratio of <see cref="HitObjectInterval"/> between this and the previous <see cref="SameRhythmHitObjectGrouping"/>. In the
        /// case where one or both of the <see cref="HitObjectInterval"/> is undefined, this will have a value of 1.
        /// </summary>
        public readonly double HitObjectIntervalRatio;

        /// <inheritdoc/>
        public double Interval { get; }

        public SameRhythmHitObjectGrouping(SameRhythmHitObjectGrouping? previous, List<TaikoDifficultyHitObject> hitObjects)
        {
            Previous = previous;
            HitObjects = hitObjects;

            // Calculate the average interval between hitobjects, or null if there are fewer than two
            HitObjectInterval = HitObjects.Count < 2 ? null : Duration / (HitObjects.Count - 1);

            // Calculate the ratio between this group's interval and the previous group's interval
            HitObjectIntervalRatio = Previous?.HitObjectInterval != null && HitObjectInterval != null
                ? HitObjectInterval.Value / Previous.HitObjectInterval.Value
                : 1;

            // Calculate the interval from the previous group's start time
            Interval = Previous != null ? StartTime - Previous.StartTime : double.PositiveInfinity;
        }
    }
}
