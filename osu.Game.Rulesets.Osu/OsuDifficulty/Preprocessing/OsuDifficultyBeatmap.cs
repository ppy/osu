// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.OsuDifficulty.Preprocessing
{
    public class OsuDifficultyBeatmap : IEnumerable<OsuDifficultyHitObject>
    {
        IEnumerator<OsuDifficultyHitObject> difficultyObjects;
        private Queue<OsuDifficultyHitObject> onScreen = new Queue<OsuDifficultyHitObject>();

        public OsuDifficultyBeatmap(List<OsuHitObject> objects)
        {
            // Sort HitObjects by StartTime - they are not correctly ordered in some cases.
            // This should probably happen before the objects reach the difficulty calculator.
            objects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            difficultyObjects = createDifficultyObjectEnumerator(objects);
        }

        public IEnumerator<OsuDifficultyHitObject> GetEnumerator()
        {
            do
            {
                // Add upcoming notes to the queue until we have at least one note that had been hit and can be dequeued.
                // This means there is always at least one note in the queue unless we reached the end of the map.
                bool hasNext;
                do
                {
                    hasNext = difficultyObjects.MoveNext();
                    if (onScreen.Count == 0 && !hasNext)
                        yield break; // Stop if we have an empty enumerator.

                    if (hasNext)
                    {
                        OsuDifficultyHitObject latest = difficultyObjects.Current;
                        // Calculate flow values here

                        foreach (OsuDifficultyHitObject h in onScreen)
                        {
                            h.MSUntilHit -= latest.MS;
                            // Calculate reading strain here
                        }

                        onScreen.Enqueue(latest);
                    }
                }
                while (onScreen.Peek().MSUntilHit > 0 && hasNext); // Keep adding new notes on screen while there is still time before we have to hit the next one.

                yield return onScreen.Dequeue(); // Remove and return notes one by one that had to be hit before the latest note appeared.
            }
            while (onScreen.Count > 0);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<OsuDifficultyHitObject> createDifficultyObjectEnumerator(List<OsuHitObject> objects)
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
