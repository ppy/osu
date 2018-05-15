// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections;
using System.Collections.Generic;
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
            objects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            difficultyObjects = createDifficultyObjectEnumerator(objects, timeRate);
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
                yield return new OsuDifficultyHitObject(objects[i], objects[i - 1], timeRate);
        }
    }
}
