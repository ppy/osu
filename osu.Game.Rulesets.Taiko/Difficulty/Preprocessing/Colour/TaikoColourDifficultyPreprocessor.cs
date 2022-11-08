// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Utility class to perform various encodings.
    /// </summary>
    public static class TaikoColourDifficultyPreprocessor
    {
        /// <summary>
        /// Processes and encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="TaikoDifficultyHitObjectColour"/>s,
        /// assigning the appropriate <see cref="TaikoDifficultyHitObjectColour"/>s to each <see cref="TaikoDifficultyHitObject"/>.
        /// </summary>
        public static void ProcessAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<RepeatingHitPatterns> hitPatterns = encode(hitObjects);

            // Assign indexing and encoding data to all relevant objects.
            foreach (var repeatingHitPattern in hitPatterns)
            {
                // The outermost loop is kept a ForEach loop since it doesn't need index information, and we want to
                // keep i and j for AlternatingMonoPattern's and MonoStreak's index respectively, to keep it in line with
                // documentation.
                for (int i = 0; i < repeatingHitPattern.AlternatingMonoPatterns.Count; ++i)
                {
                    AlternatingMonoPattern monoPattern = repeatingHitPattern.AlternatingMonoPatterns[i];
                    monoPattern.Parent = repeatingHitPattern;
                    monoPattern.Index = i;

                    for (int j = 0; j < monoPattern.MonoStreaks.Count; ++j)
                    {
                        MonoStreak monoStreak = monoPattern.MonoStreaks[j];
                        monoStreak.Parent = monoPattern;
                        monoStreak.Index = j;

                        foreach (var hitObject in monoStreak.HitObjects)
                        {
                            hitObject.Colour.RepeatingHitPattern = repeatingHitPattern;
                            hitObject.Colour.AlternatingMonoPattern = monoPattern;
                            hitObject.Colour.MonoStreak = monoStreak;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="RepeatingHitPatterns"/>s.
        /// </summary>
        private static List<RepeatingHitPatterns> encode(List<DifficultyHitObject> data)
        {
            List<MonoStreak> monoStreaks = encodeMonoStreak(data);
            List<AlternatingMonoPattern> alternatingMonoPatterns = encodeAlternatingMonoPattern(monoStreaks);
            List<RepeatingHitPatterns> repeatingHitPatterns = encodeRepeatingHitPattern(alternatingMonoPatterns);

            return repeatingHitPatterns;
        }

        /// <summary>
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="MonoStreak"/>s.
        /// </summary>
        private static List<MonoStreak> encodeMonoStreak(List<DifficultyHitObject> data)
        {
            List<MonoStreak> monoStreaks = new List<MonoStreak>();
            MonoStreak? currentMonoStreak = null;

            for (int i = 0; i < data.Count; i++)
            {
                TaikoDifficultyHitObject taikoObject = (TaikoDifficultyHitObject)data[i];

                // This ignores all non-note objects, which may or may not be the desired behaviour
                TaikoDifficultyHitObject? previousObject = taikoObject.PreviousNote(0);

                // If this is the first object in the list or the colour changed, create a new mono streak
                if (currentMonoStreak == null || previousObject == null || (taikoObject.BaseObject as Hit)?.Type != (previousObject.BaseObject as Hit)?.Type)
                {
                    currentMonoStreak = new MonoStreak();
                    monoStreaks.Add(currentMonoStreak);
                }

                // Add the current object to the encoded payload.
                currentMonoStreak.HitObjects.Add(taikoObject);
            }

            return monoStreaks;
        }

        /// <summary>
        /// Encodes a list of <see cref="MonoStreak"/>s into a list of <see cref="AlternatingMonoPattern"/>s.
        /// </summary>
        private static List<AlternatingMonoPattern> encodeAlternatingMonoPattern(List<MonoStreak> data)
        {
            List<AlternatingMonoPattern> monoPatterns = new List<AlternatingMonoPattern>();
            AlternatingMonoPattern? currentMonoPattern = null;

            for (int i = 0; i < data.Count; i++)
            {
                // Start a new AlternatingMonoPattern if the previous MonoStreak has a different mono length, or if this is the first MonoStreak in the list.
                if (currentMonoPattern == null || data[i].RunLength != data[i - 1].RunLength)
                {
                    currentMonoPattern = new AlternatingMonoPattern();
                    monoPatterns.Add(currentMonoPattern);
                }

                // Add the current MonoStreak to the encoded payload.
                currentMonoPattern.MonoStreaks.Add(data[i]);
            }

            return monoPatterns;
        }

        /// <summary>
        /// Encodes a list of <see cref="AlternatingMonoPattern"/>s into a list of <see cref="RepeatingHitPatterns"/>s.
        /// </summary>
        private static List<RepeatingHitPatterns> encodeRepeatingHitPattern(List<AlternatingMonoPattern> data)
        {
            List<RepeatingHitPatterns> hitPatterns = new List<RepeatingHitPatterns>();
            RepeatingHitPatterns? currentHitPattern = null;

            for (int i = 0; i < data.Count; i++)
            {
                // Start a new RepeatingHitPattern. AlternatingMonoPatterns that should be grouped together will be handled later within this loop.
                currentHitPattern = new RepeatingHitPatterns(currentHitPattern);

                // Determine if future AlternatingMonoPatterns should be grouped.
                bool isCoupled = i < data.Count - 2 && data[i].IsRepetitionOf(data[i + 2]);

                if (!isCoupled)
                {
                    // If not, add the current AlternatingMonoPattern to the encoded payload and continue.
                    currentHitPattern.AlternatingMonoPatterns.Add(data[i]);
                }
                else
                {
                    // If so, add the current AlternatingMonoPattern to the encoded payload and start repeatedly checking if the
                    // subsequent AlternatingMonoPatterns should be grouped by increasing i and doing the appropriate isCoupled check.
                    while (isCoupled)
                    {
                        currentHitPattern.AlternatingMonoPatterns.Add(data[i]);
                        i++;
                        isCoupled = i < data.Count - 2 && data[i].IsRepetitionOf(data[i + 2]);
                    }

                    // Skip over viewed data and add the rest to the payload
                    currentHitPattern.AlternatingMonoPatterns.Add(data[i]);
                    currentHitPattern.AlternatingMonoPatterns.Add(data[i + 1]);
                    i++;
                }

                hitPatterns.Add(currentHitPattern);
            }

            // Final pass to find repetition intervals
            for (int i = 0; i < hitPatterns.Count; i++)
            {
                hitPatterns[i].FindRepetitionInterval();
            }

            return hitPatterns;
        }
    }
}
