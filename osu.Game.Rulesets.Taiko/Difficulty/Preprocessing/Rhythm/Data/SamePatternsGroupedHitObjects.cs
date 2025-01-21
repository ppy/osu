// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Represents <see cref="SameRhythmGroupedHitObjects"/> grouped by their <see cref="SameRhythmGroupedHitObjects.StartTime"/>'s interval.
    /// </summary>
    public class SamePatternsGroupedHitObjects
    {
        public IReadOnlyList<SameRhythmGroupedHitObjects> Children { get; }

        public SamePatternsGroupedHitObjects? Previous { get; }

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

        public SamePatternsGroupedHitObjects(SamePatternsGroupedHitObjects? previous, List<SameRhythmGroupedHitObjects> children)
        {
            Previous = previous;
            Children = children;
        }
    }
}
