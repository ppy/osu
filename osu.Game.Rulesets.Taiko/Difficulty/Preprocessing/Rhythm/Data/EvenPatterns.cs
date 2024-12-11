// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Represents <see cref="EvenHitObjects"/> grouped by their <see cref="EvenHitObjects.StartTime"/>'s interval.
    /// </summary>
    public class EvenPatterns : EvenRhythm<EvenHitObjects>
    {
        public EvenPatterns? Previous { get; private set; }

        /// <summary>
        /// The <see cref="EvenHitObjects.Interval"/> between children <see cref="EvenHitObjects"/> within this group.
        /// If there is only one child, this will have the value of the first child's <see cref="EvenHitObjects.Interval"/>.
        /// </summary>
        public double ChildrenInterval => Children.Count > 1 ? Children[1].Interval : Children[0].Interval;

        /// <summary>
        /// The ratio of <see cref="ChildrenInterval"/> between this and the previous <see cref="EvenPatterns"/>. In the
        /// case where there is no previous <see cref="EvenPatterns"/>, this will have a value of 1.
        /// </summary>
        public double IntervalRatio => ChildrenInterval / Previous?.ChildrenInterval ?? 1.0d;

        public TaikoDifficultyHitObject FirstHitObject => Children[0].FirstHitObject;

        public IEnumerable<TaikoDifficultyHitObject> AllHitObjects => Children.SelectMany(child => child.Children);

        private EvenPatterns(EvenPatterns? previous, List<EvenHitObjects> data, ref int i)
            : base(data, ref i, 5)
        {
            Previous = previous;

            foreach (TaikoDifficultyHitObject hitObject in AllHitObjects)
            {
                hitObject.Rhythm.EvenPatterns = this;
            }
        }

        public static void GroupPatterns(List<EvenHitObjects> data)
        {
            List<EvenPatterns> evenPatterns = new List<EvenPatterns>();

            // Index does not need to be incremented, as it is handled within the EvenRhythm constructor.
            for (int i = 0; i < data.Count;)
            {
                EvenPatterns? previous = evenPatterns.Count > 0 ? evenPatterns[^1] : null;
                evenPatterns.Add(new EvenPatterns(previous, data, ref i));
            }
        }
    }
}
