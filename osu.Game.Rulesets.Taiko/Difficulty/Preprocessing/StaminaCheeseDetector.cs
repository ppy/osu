// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
            List<TaikoDifficultyHitObject> history = new List<TaikoDifficultyHitObject>();

            int repetitionStart = 0;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                history.Add(hitObjects[i]);
                if (history.Count < 2 * patternLength) continue;

                if (history.Count > 2 * patternLength) history.RemoveAt(0);

                bool isRepeat = true;

                for (int j = 0; j < patternLength; j++)
                {
                    if (history[j].HitType != history[j + patternLength].HitType)
                    {
                        isRepeat = false;
                    }
                }

                if (!isRepeat)
                {
                    repetitionStart = i - 2 * patternLength;
                }

                int repeatedLength = i - repetitionStart;

                if (repeatedLength >= roll_min_repetitions)
                {
                    for (int j = repetitionStart; j < i; j++)
                    {
                        hitObjects[j].StaminaCheese = true;
                    }
                }
            }
        }

        private void findTlTap(int parity, HitType type)
        {
            int tlLength = -2;

            for (int i = parity; i < hitObjects.Count; i += 2)
            {
                if (hitObjects[i].HitType == type)
                {
                    tlLength += 2;
                }
                else
                {
                    tlLength = -2;
                }

                if (tlLength >= tl_min_repetitions)
                {
                    for (int j = Math.Max(0, i - tlLength); j < i; j++)
                    {
                        hitObjects[j].StaminaCheese = true;
                    }
                }
            }
        }
    }
}
