// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class StaminaCheeseDetector
    {
        private const int roll_min_repetitions = 12;
        private const int tl_min_repetitions = 16;

        private List<TaikoDifficultyHitObject> hitObjects;

        public void FindCheese(List<TaikoDifficultyHitObject> difficultyHitObjects)
        {
            hitObjects = difficultyHitObjects;
            findRolls(3);
            findRolls(4);
            findTlTap(0, HitType.Rim);
            findTlTap(1, HitType.Rim);
            findTlTap(0, HitType.Centre);
            findTlTap(1, HitType.Centre);
        }

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

        private static bool containsPatternRepeat(LimitedCapacityQueue<TaikoDifficultyHitObject> history, int patternLength)
        {
            for (int j = 0; j < patternLength; j++)
            {
                if (history[j].HitType != history[j + patternLength].HitType)
                    return false;
            }

            return true;
        }

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

        private void markObjectsAsCheese(int start, int end)
        {
            for (int i = start; i < end; ++i)
                hitObjects[i].StaminaCheese = true;
        }
    }
}
