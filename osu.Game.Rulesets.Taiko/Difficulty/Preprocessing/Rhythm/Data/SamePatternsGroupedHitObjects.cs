// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Represents <see cref="SameRhythmGroupedHitObjects"/> grouped by their <see cref="SameRhythmGroupedHitObjects.StartTime"/>'s interval.
    /// </summary>
    public class SamePatternsGroupedHitObjects : IntervalGroupedHitObjects<SameRhythmGroupedHitObjects>
    {
        public SamePatternsGroupedHitObjects? Previous { get; private set; }

        /// <summary>
        /// The <see cref="SameRhythmGroupedHitObjects.Interval"/> between children <see cref="SameRhythmGroupedHitObjects"/> within this group.
        /// If there is only one child, this will have the value of the first child's <see cref="SameRhythmGroupedHitObjects.Interval"/>.
        /// </summary>
        public double ChildrenInterval => Children.Count > 1 ? Children[1].Interval : Children[0].Interval;

        /// <summary>
        /// The ratio of <see cref="ChildrenInterval"/> between this and the previous <see cref="SamePatternsGroupedHitObjects"/>. In the
        /// case where there is no previous <see cref="SamePatternsGroupedHitObjects"/>, this will have a value of 1.
        /// </summary>
        public double IntervalRatio => ChildrenInterval / Previous?.ChildrenInterval ?? 1.0d;

        public TaikoDifficultyHitObject FirstHitObject => Children[0].FirstHitObject;

        public IEnumerable<TaikoDifficultyHitObject> AllHitObjects => Children.SelectMany(child => child.Children);

        private SamePatternsGroupedHitObjects(SamePatternsGroupedHitObjects? previous, List<SameRhythmGroupedHitObjects> data, ref int i)
            : base(data, ref i, 5)
        {
            Previous = previous;

            foreach (TaikoDifficultyHitObject hitObject in AllHitObjects)
            {
                hitObject.Rhythm.SamePatternsGroupedHitObjects = this;
            }
        }

        public static void GroupPatterns(List<SameRhythmGroupedHitObjects> data)
        {
            List<SamePatternsGroupedHitObjects> samePatterns = new List<SamePatternsGroupedHitObjects>();

            // Index does not need to be incremented, as it is handled within the IntervalGroupedHitObjects constructor.
            for (int i = 0; i < data.Count;)
            {
                SamePatternsGroupedHitObjects? previous = samePatterns.Count > 0 ? samePatterns[^1] : null;
                samePatterns.Add(new SamePatternsGroupedHitObjects(previous, data, ref i));
            }
        }
    }
}
