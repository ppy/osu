// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Represents a group of <see cref="TaikoDifficultyHitObject"/>s with no rhythm variation.
    /// </summary>
    public class SameRhythmGroupedHitObjects : IntervalGroupedHitObjects<TaikoDifficultyHitObject>, IHasInterval
    {
        public TaikoDifficultyHitObject FirstHitObject => Children[0];

        public SameRhythmGroupedHitObjects? Previous;

        /// <summary>
        /// <see cref="DifficultyHitObject.StartTime"/> of the first hit object.
        /// </summary>
        public double StartTime => Children[0].StartTime;

        /// <summary>
        /// The interval between the first and final hit object within this group.
        /// </summary>
        public double Duration => Children[^1].StartTime - Children[0].StartTime;

        /// <summary>
        /// The interval in ms of each hit object in this <see cref="SameRhythmGroupedHitObjects"/>. This is only defined if there is
        /// more than two hit objects in this <see cref="SameRhythmGroupedHitObjects"/>.
        /// </summary>
        public double? HitObjectInterval;

        /// <summary>
        /// The ratio of <see cref="HitObjectInterval"/> between this and the previous <see cref="SameRhythmGroupedHitObjects"/>. In the
        /// case where one or both of the <see cref="HitObjectInterval"/> is undefined, this will have a value of 1.
        /// </summary>
        public double HitObjectIntervalRatio = 1;

        /// <inheritdoc/>
        public double Interval { get; private set; }

        public SameRhythmGroupedHitObjects(SameRhythmGroupedHitObjects? previous, List<TaikoDifficultyHitObject> data, ref int i)
            : base(data, ref i, 5)
        {
            Previous = previous;

            foreach (var hitObject in Children)
            {
                hitObject.Rhythm.SameRhythmGroupedHitObjects = this;

                // Pass the HitObjectInterval to each child.
                hitObject.HitObjectInterval = HitObjectInterval;
            }

            calculateIntervals();
        }

        public static List<SameRhythmGroupedHitObjects> GroupHitObjects(List<TaikoDifficultyHitObject> data)
        {
            List<SameRhythmGroupedHitObjects> flatPatterns = new List<SameRhythmGroupedHitObjects>();

            // Index does not need to be incremented, as it is handled within IntervalGroupedHitObjects's constructor.
            for (int i = 0; i < data.Count;)
            {
                SameRhythmGroupedHitObjects? previous = flatPatterns.Count > 0 ? flatPatterns[^1] : null;
                flatPatterns.Add(new SameRhythmGroupedHitObjects(previous, data, ref i));
            }

            return flatPatterns;
        }

        private void calculateIntervals()
        {
            // Calculate the average interval between hitobjects, or null if there are fewer than two.
            HitObjectInterval = Children.Count < 2 ? null : (Children[^1].StartTime - Children[0].StartTime) / (Children.Count - 1);

            // If both the current and previous intervals are available, calculate the ratio.
            if (Previous?.HitObjectInterval != null && HitObjectInterval != null)
            {
                HitObjectIntervalRatio = HitObjectInterval.Value / Previous.HitObjectInterval.Value;
            }

            if (Previous == null)
            {
                return;
            }

            Interval = StartTime - Previous.StartTime;
        }
    }
}
