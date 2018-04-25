// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.OsuDifficulty.Preprocessing
{
    /// <summary>
    /// An enumerable container wrapping <see cref="OsuHitObject"/> input as <see cref="OsuDifficultyHitObject"/>
    /// which contains extra data required for difficulty calculation.
    /// </summary>
    public class OsuDifficultyBeatmap : IEnumerable<OsuDifficultyHitObject>
    {
        private readonly IEnumerator<OsuDifficultyHitObject> difficultyObjects;
        private readonly Queue<OsuDifficultyHitObject> onScreen = new Queue<OsuDifficultyHitObject>();

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
        /// The inner loop adds objects that appear on screen into a queue until we need to hit the next object.
        /// The outer loop returns objects from this queue one at a time, only after they had to be hit, and should no longer be on screen.
        /// This means that we can loop through every object that is on screen at the time when a new one appears,
        /// allowing us to determine a reading strain for the object that just appeared.
        /// </summary>
        public IEnumerator<OsuDifficultyHitObject> GetEnumerator()
        {
            while (true)
            {
                // Add upcoming objects to the queue until we have at least one object that had been hit and can be dequeued.
                // This means there is always at least one object in the queue unless we reached the end of the map.
                do
                {
                    if (!difficultyObjects.MoveNext())
                        break; // New objects can't be added anymore, but we still need to dequeue and return the ones already on screen.

                    OsuDifficultyHitObject latest = difficultyObjects.Current;
                    // Calculate flow values here

                    foreach (OsuDifficultyHitObject h in onScreen)
                    {
                        // ReSharper disable once PossibleNullReferenceException (resharper not smart enough to understand IEnumerator.MoveNext())
                        h.TimeUntilHit -= latest.DeltaTime;
                        // Calculate reading strain here
                    }

                    onScreen.Enqueue(latest);
                }
                while (onScreen.Peek().TimeUntilHit > 0); // Keep adding new objects on screen while there is still time before we have to hit the next one.

                if (onScreen.Count == 0) break; // We have reached the end of the map and enumerated all the objects.
                yield return onScreen.Dequeue(); // Remove and return objects one by one that had to be hit before the latest one appeared.
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerator<OsuDifficultyHitObject> createDifficultyObjectEnumerator(List<OsuHitObject> objects, double timeRate)
        {
            // We will process OsuHitObjects in groups of three to form a triangle, so we can calculate an angle for each object.
            OsuHitObject[] triangle = new OsuHitObject[3];

            // OsuDifficultyHitObject construction requires three components, an extra copy of the first OsuHitObject is used at the beginning.
            if (objects.Count > 1)
            {
                triangle[1] = objects[0]; // This copy will get shifted to the last spot in the triangle.
                triangle[0] = objects[0]; // This component corresponds to the real first OsuHitOject.
            }

            // The final component of the first triangle will be the second OsuHitOject of the map, which forms the first jump.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < objects.Count; ++i)
            {
                triangle[2] = triangle[1];
                triangle[1] = triangle[0];
                triangle[0] = objects[i];

                yield return new OsuDifficultyHitObject(triangle, timeRate);
            }
        }
    }
}
