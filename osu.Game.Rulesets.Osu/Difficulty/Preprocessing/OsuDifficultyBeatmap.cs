// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    /// <summary>
    /// An enumerable container wrapping <see cref="OsuHitObject"/> input as <see cref="OsuDifficultyHitObject"/>
    /// which contains extra data required for difficulty calculation.
    /// </summary>
    public class OsuDifficultyBeatmap : IEnumerable<OsuDifficultyHitObject>
    {
        private readonly IEnumerator<OsuDifficultyHitObject> difficultyObjects;

        /// <summary>
        /// Creates an enumerator, which preprocesses a list of <see cref="OsuHitObject"/>s recieved as input, wrapping them as
        /// <see cref="OsuDifficultyHitObject"/> which contains extra data required for difficulty calculation.
        /// </summary>
        public OsuDifficultyBeatmap(List<OsuHitObject> objects, double timeRate)
        {
            // Sort OsuHitObjects by StartTime - they are not correctly ordered in some cases.
            // This should probably happen before the objects reach the difficulty calculator.
            difficultyObjects = createDifficultyObjectEnumerator(objects.OrderBy(h => h.StartTime).ToList(), timeRate);
        }

        /// <summary>
        /// Returns an enumerator that enumerates all <see cref="OsuDifficultyHitObject"/>s in the <see cref="OsuDifficultyBeatmap"/>.
        /// </summary>
        public IEnumerator<OsuDifficultyHitObject> GetEnumerator() => difficultyObjects;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerator<OsuDifficultyHitObject> createDifficultyObjectEnumerator(List<OsuHitObject> objects, double timeRate)
        {
            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < objects.Count; i++)
            {
                var lastLast = i > 1 ? objects[i - 2] : null;
                var last = objects[i - 1];
                var current = objects[i];

                yield return new OsuDifficultyHitObject(lastLast, last, current, timeRate);
            }
        }
    }
}
