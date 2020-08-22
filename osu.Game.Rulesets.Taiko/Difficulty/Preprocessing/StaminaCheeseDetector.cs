// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    /// <summary>
    /// Detects special hit object patterns which are easier to hit using special techniques
    /// than normally assumed in the fully-alternating play style.
    /// </summary>
    /// <remarks>
    /// This component detects two basic types of patterns, leveraged by the following techniques:
    /// <list>
    /// <item>Rolling allows hitting patterns with quickly and regularly alternating notes with a single hand.</item>
    /// <item>TL tapping makes hitting longer sequences of consecutive same-colour notes with little to no colour changes in-between.</item>
    /// </list>
    /// </remarks>
    public class StaminaCheeseDetector
    {
        /// <summary>
        /// The minimum number of consecutive objects with repeating patterns that can be classified as hittable using a roll.
        /// </summary>
        private const int roll_min_repetitions = 12;

        /// <summary>
        /// The minimum number of consecutive objects with repeating patterns that can be classified as hittable using a TL tap.
        /// </summary>
        private const int tl_min_repetitions = 16;

        /// <summary>
        /// The list of all <see cref="TaikoDifficultyHitObject"/>s in the map.
        /// </summary>
        private readonly List<TaikoDifficultyHitObject> hitObjects;

        public StaminaCheeseDetector(List<TaikoDifficultyHitObject> hitObjects)
        {
            this.hitObjects = hitObjects;
        }

        /// <summary>
        /// Finds and marks all objects in <see cref="hitObjects"/> that special difficulty-reducing techiques apply to
        /// with the <see cref="TaikoDifficultyHitObject.StaminaCheese"/> flag.
        /// </summary>
        public void FindCheese()
        {
            findRolls(3);
            findRolls(4);

            findTlTap(0, HitType.Rim);
            findTlTap(1, HitType.Rim);
            findTlTap(0, HitType.Centre);
            findTlTap(1, HitType.Centre);
        }

        /// <summary>
        /// Finds and marks all sequences hittable using a roll.
        /// </summary>
        /// <param name="patternLength">The length of a single repeating pattern to consider (triplets/quadruplets).</param>
        private void findRolls(int patternLength)
        {
            var history = new LimitedCapacityQueue<TaikoDifficultyHitObject>(2 * patternLength);

            int repetitionStart = 0;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                history.Enqueue(hitObjects[i]);
                if (!history.Full)
                    continue;

                if (!containsPatternRepeat(history, patternLength))
                {
                    repetitionStart = i - 2 * patternLength;
                    continue;
                }

                int repeatedLength = i - repetitionStart;
                if (repeatedLength < roll_min_repetitions)
                    continue;

                markObjectsAsCheese(repetitionStart, i);
            }
        }

        /// <summary>
        /// Determines whether the objects stored in <paramref name="history"/> contain a repetition of a pattern of length <paramref name="patternLength"/>.
        /// </summary>
        private static bool containsPatternRepeat(LimitedCapacityQueue<TaikoDifficultyHitObject> history, int patternLength)
        {
            for (int j = 0; j < patternLength; j++)
            {
                if (history[j].HitType != history[j + patternLength].HitType)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Finds and marks all sequences hittable using a TL tap.
        /// </summary>
        /// <param name="parity">Whether sequences starting with an odd- (1) or even-indexed (0) hit object should be checked.</param>
        /// <param name="type">The type of hit to check for TL taps.</param>
        private void findTlTap(int parity, HitType type)
        {
            int tlLength = -2;

            for (int i = parity; i < hitObjects.Count; i += 2)
            {
                if (hitObjects[i].HitType == type)
                    tlLength += 2;
                else
                    tlLength = -2;

                if (tlLength < tl_min_repetitions)
                    continue;

                markObjectsAsCheese(Math.Max(0, i - tlLength), i);
            }
        }

        /// <summary>
        /// Marks all objects from index <paramref name="start"/> up until <paramref name="end"/> (exclusive) as <see cref="TaikoDifficultyHitObject.StaminaCheese"/>.
        /// </summary>
        private void markObjectsAsCheese(int start, int end)
        {
            for (int i = start; i < end; ++i)
                hitObjects[i].StaminaCheese = true;
        }
    }
}
