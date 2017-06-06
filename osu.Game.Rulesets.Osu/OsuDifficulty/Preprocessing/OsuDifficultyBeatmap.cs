// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.OsuDifficulty.Preprocessing
{
    public class OsuDifficultyBeatmap : IEnumerable<OsuDifficultyHitObject>
    {
        private readonly IEnumerator<OsuDifficultyHitObject> difficultyObjects;
        private readonly Queue<OsuDifficultyHitObject> onScreen = new Queue<OsuDifficultyHitObject>();

        /// <summary>
        /// Creates an enumerable, which handles the preprocessing of HitObjects, getting them ready to be used in difficulty calculation.
        /// </summary>
        public OsuDifficultyBeatmap(List<OsuHitObject> objects)
        {
            // Sort HitObjects by StartTime - they are not correctly ordered in some cases.
            // This should probably happen before the objects reach the difficulty calculator.
            objects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            difficultyObjects = createDifficultyObjectEnumerator(objects);
        }

        /// <summary>
        /// Returns an enumerator that enumerates all difficulty objects in the beatmap.
        /// The inner loop adds notes that appear on screen into a queue until we need to hit the next note,
        /// the outer loop returns notes from this queue one at a time, only after they had to be hit, and should no longer be on screen.
        /// This means that we can loop through every object that is on screen at the time when a new one appears,
        /// allowing us to determine a reading strain for the note that just appeared.
        /// </summary>
        public IEnumerator<OsuDifficultyHitObject> GetEnumerator()
        {
            while (true)
            {
                // Add upcoming notes to the queue until we have at least one note that had been hit and can be dequeued.
                // This means there is always at least one note in the queue unless we reached the end of the map.
                do
                {
                    if (!difficultyObjects.MoveNext())
                        break; // New notes can't be added anymore, but we still need to dequeue and return the ones already on screen.

                    OsuDifficultyHitObject latest = difficultyObjects.Current;
                    // Calculate flow values here

                    foreach (OsuDifficultyHitObject h in onScreen)
                    {
                        h.TimeUntilHit -= latest.DeltaTime;
                        // Calculate reading strain here
                    }

                    onScreen.Enqueue(latest);
                }
                while (onScreen.Peek().TimeUntilHit > 0); // Keep adding new notes on screen while there is still time before we have to hit the next one.

                if (onScreen.Count == 0) break; // We have reached the end of the map and enumerated all the notes.
                yield return onScreen.Dequeue(); // Remove and return notes one by one that had to be hit before the latest note appeared.
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerator<OsuDifficultyHitObject> createDifficultyObjectEnumerator(List<OsuHitObject> objects)
        {
            // We will process HitObjects in groups of three to form a triangle, so we can calculate an angle for each note.
            OsuHitObject[] triangle = new OsuHitObject[3];

            // Difficulty object construction requires three components, an extra copy of the first object is used at the beginning.
            if (objects.Count > 1)
            {
                triangle[1] = objects[0]; // This copy will get shifted to the last spot in the triangle.
                triangle[0] = objects[0]; // This is the real first note.
            }

            // The final component of the first triangle will be the second note, which forms the first jump.
            // If the beatmap has less than two HitObjects, the enumerator will not return anything.
            for (int i = 1; i < objects.Count; ++i)
            {
                triangle[2] = triangle[1];
                triangle[1] = triangle[0];
                triangle[0] = objects[i];

                yield return new OsuDifficultyHitObject(triangle);
            }
        }
    }
}
