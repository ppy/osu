// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

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
            findTlTap(0, true);
            findTlTap(1, true);
            findTlTap(0, false);
            findTlTap(1, false);
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
                    if (history[j].IsKat != history[j + patternLength].IsKat)
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
                        hitObjects[i].StaminaCheese = true;
                    }
                }
            }
        }

        private void findTlTap(int parity, bool kat)
        {
            int tlLength = -2;

            for (int i = parity; i < hitObjects.Count; i += 2)
            {
                if (kat == hitObjects[i].IsKat)
                {
                    tlLength += 2;
                }
                else
                {
                    tlLength = -2;
                }

                if (tlLength >= tl_min_repetitions)
                {
                    for (int j = i - tlLength; j < i; j++)
                    {
                        hitObjects[i].StaminaCheese = true;
                    }
                }
            }
        }
    }
}
