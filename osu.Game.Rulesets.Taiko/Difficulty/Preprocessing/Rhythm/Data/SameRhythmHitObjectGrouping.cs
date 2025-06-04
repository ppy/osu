// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// The normalised interval in ms of each hit object in this <see cref="SameRhythmHitObjectGrouping"/>. This is only defined if there is
        /// more than two hit objects in this <see cref="SameRhythmHitObjectGrouping"/>.
        /// </summary>
        public readonly double? HitObjectInterval;

        /// <summary>
        /// The normalised ratio of <see cref="HitObjectInterval"/> between this and the previous <see cref="SameRhythmHitObjectGrouping"/>. In the
        /// case where one or both of the <see cref="HitObjectInterval"/> is undefined, this will have a value of 1.
        /// </summary>
        public readonly double HitObjectIntervalRatio;

        private const double snap_tolerance = 5.0; // Tolerance for snapping intervals to the previous group in ms.

        /// <inheritdoc/>
        public double Interval { get; }

        public SameRhythmHitObjectGrouping(SameRhythmHitObjectGrouping? previous, List<TaikoDifficultyHitObject> hitObjects)
        {
            Previous = previous;
            HitObjects = hitObjects;

            // Cluster and normalise each hitobjects delta-time.
            var normaliseHitObjects = DeltaTimeNormaliser.Normalise(hitObjects, 5.0);

            var normalisedhitObjectDeltaTime = hitObjects
                                               .Skip(1)
                                               .Select(hitObject => normaliseHitObjects[hitObject])
                                               .ToList();

            double modalDelta = normalisedhitObjectDeltaTime.Count > 0
                ? normalisedhitObjectDeltaTime
                  .Select(deltaTime => Math.Round(deltaTime))
                  .GroupBy(deltaTime => deltaTime)
                  .OrderByDescending(group => group.Count())
                  .First().Key
                : 0;

            // Calculate the average interval between hitobjects.
            HitObjectInterval = normalisedhitObjectDeltaTime.Count > 0
                ? previous?.HitObjectInterval is double previousDelta && Math.Abs(modalDelta - previousDelta) <= snap_tolerance
                    ? previousDelta
                    : modalDelta
                : null;

            // Calculate the ratio between this group's interval and the previous group's interval
            HitObjectIntervalRatio = previous?.HitObjectInterval is double previousInterval && HitObjectInterval is double currentInterval
                ? currentInterval / previousInterval
                : 1.0;

            // Calculate the interval from the previous group's start time
            Interval = previous == null
                ? double.PositiveInfinity
                : Math.Abs(StartTime - previous.StartTime) <= snap_tolerance
                    ? 0
                    : StartTime - previous.StartTime;
        }
    }
}
