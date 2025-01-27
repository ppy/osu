// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Represents <see cref="SameRhythmHitObjectGrouping"/> grouped by their <see cref="SameRhythmHitObjectGrouping.StartTime"/>'s interval.
    /// </summary>
    public class SamePatternsGroupedHitObjects
    {
        public IReadOnlyList<SameRhythmHitObjectGrouping> Groups { get; }

        public SamePatternsGroupedHitObjects? Previous { get; }

        /// <summary>
        /// The <see cref="SameRhythmHitObjectGrouping.Interval"/> between groups <see cref="SameRhythmHitObjectGrouping"/>.
        /// If there is only one group, this will have the value of the first group's <see cref="SameRhythmHitObjectGrouping.Interval"/>.
        /// </summary>
        public double GroupInterval => Groups.Count > 1 ? Groups[1].Interval : Groups[0].Interval;

        /// <summary>
        /// The ratio of <see cref="GroupInterval"/> between this and the previous <see cref="SamePatternsGroupedHitObjects"/>. In the
        /// case where there is no previous <see cref="SamePatternsGroupedHitObjects"/>, this will have a value of 1.
        /// </summary>
        public double IntervalRatio => GroupInterval / Previous?.GroupInterval ?? 1.0d;

        public TaikoDifficultyHitObject FirstHitObject => Groups[0].FirstHitObject;

        public IEnumerable<TaikoDifficultyHitObject> AllHitObjects => Groups.SelectMany(hitObject => hitObject.HitObjects);

        public SamePatternsGroupedHitObjects(SamePatternsGroupedHitObjects? previous, List<SameRhythmHitObjectGrouping> groups)
        {
            Previous = previous;
            Groups = groups;
        }
    }
}
