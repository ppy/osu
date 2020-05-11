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
            this.hitObjects = difficultyHitObjects;
            findRolls(3);
            findRolls(4);
            findTLTap(0, true);
            findTLTap(1, true);
            findTLTap(0, false);
            findTLTap(1, false);
        }

        private void findRolls(int patternLength)
        {
            List<TaikoDifficultyHitObject> history = new List<TaikoDifficultyHitObject>();

            int repititionStart = 0;

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
                    repititionStart = i - 2 * patternLength;
                }

                int repeatedLength = i - repititionStart;

                if (repeatedLength >= roll_min_repetitions)
                {
                    // Console.WriteLine("Found Roll Cheese.\tStart: " + repititionStart + "\tEnd: " + i);
                    for (int j = repititionStart; j < i; j++)
                    {
                        (hitObjects[i]).StaminaCheese = true;
                    }
                }
            }
        }

        private void findTLTap(int parity, bool kat)
        {
            int tl_length = -2;

            for (int i = parity; i < hitObjects.Count; i += 2)
            {
                if (kat == hitObjects[i].IsKat)
                {
                    tl_length += 2;
                }
                else
                {
                    tl_length = -2;
                }

                if (tl_length >= tl_min_repetitions)
                {
                    // Console.WriteLine("Found TL Cheese.\tStart: " + (i - tl_length) + "\tEnd: " + i);
                    for (int j = i - tl_length; j < i; j++)
                    {
                        (hitObjects[i]).StaminaCheese = true;
                    }
                }
            }
        }
    }
}
