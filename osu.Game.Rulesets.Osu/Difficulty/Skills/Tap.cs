// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Tap : OsuSkill
    {
        protected override double StarsPerDouble => 1.075;
        protected override int HistoryLength => 16;
        protected override int decayExcessThreshold => 500;
        protected override double baseDecay => 0.9;

        private int strainTimeBuffRange = 75;

        private double currentStrain = 1;
        // Global Tap Strain Multiplier.
        private double strainMultiplier = 2.575;
        private double rhythmMultiplier = 1;

        public Tap(Mod[] mods)
            : base(mods)
        {
        }

        private bool isRatioEqual(double ratio, double a, double b)
        {
            return a + 15 > ratio * b && a - 15 < ratio * b;
        }

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double calculateRhythmDifficulty()
        {
            // {doubles, triplets, quads, quints, 6-tuplets, 7 Tuplets, greater}
            double[] islandSizes = {0, 0, 0, 0, 0, 0, 0};
            double[] islandTimes = {0, 0, 0, 0, 0, 0, 0};
            int islandSize = 0;
            double specialTransitionCount = 0;

            bool firstDeltaSwitch = false;

            for (int i = 1; i < Previous.Count; i++)
            {
                double prevDelta = ((OsuDifficultyHitObject)Previous[i - 1]).StrainTime;
                double currDelta = ((OsuDifficultyHitObject)Previous[i]).StrainTime;

                if (isRatioEqual(1.5, prevDelta, currDelta) || isRatioEqual(1.5, currDelta, prevDelta))
                {
                    if (Previous[i - 1].BaseObject is Slider || Previous[i].BaseObject is Slider)
                        specialTransitionCount += 50.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                    else
                        specialTransitionCount += 250.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                }

                if (firstDeltaSwitch)
                {
                    if (isRatioEqual(1.0, prevDelta, currDelta))
                    {
                        islandSize++; // island is still progressing, count size.
                    }
                    else if (prevDelta > currDelta * 1.25) // we're speeding up
                    {
                        if (islandSize > 6)
                        {
                            islandTimes[6] = islandTimes[6] + 100.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                            islandSizes[6] = islandSizes[6] + 1;
                        }
                        else
                        {
                            islandTimes[islandSize] = islandTimes[islandSize] + 100.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                            islandSizes[islandSize] = islandSizes[islandSize] + 1;
                        }

                        islandSize = 0; // reset and count again, we sped up (usually this could only be if we did a 1/2 -> 1/3 -> 1/4) (or 1/1 -> 1/2 -> 1/4)
                    }
                    else // we're not the same or speeding up, must be slowing down.
                    {
                        if (islandSize > 6)
                        {
                            islandTimes[6] = islandTimes[6] + 100.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                            islandSizes[6] = islandSizes[6] + 1;
                        }
                        else
                        {
                            islandTimes[islandSize] = islandTimes[islandSize] + 100.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength) ;
                            islandSizes[islandSize] = islandSizes[islandSize] + 1;
                        }

                        firstDeltaSwitch = false; // stop counting island until next speed up.
                    }
                }
                else if (prevDelta >  1.25 * currDelta) // we want to be speeding up.
                {
                    // Begin counting island until we slow again.
                    firstDeltaSwitch = true;
                    islandSize = 0;
                }
            }

            double rhythmComplexitySum = 0.0;

            for (int i = 0; i < islandSizes.Length; i++)
            {
                if (islandSizes[i] != 0)
                    rhythmComplexitySum += islandTimes[i] / Math.Pow(islandSizes[i], .5); // sum the total amount of rhythm variance, penalizing for repeated island sizes.
            }

            rhythmComplexitySum += specialTransitionCount; // add in our 1.5 * transitions

            return Math.Sqrt(4 + rhythmComplexitySum * rhythmMultiplier) / 2;
        }

        protected override double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count == 0)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double strainValue = .25;

            double avgDeltaTime = (osuCurrent.StrainTime + ((OsuDifficultyHitObject)Previous[0]).StrainTime) / 2;

            double rhythmComplexity = calculateRhythmDifficulty(); // equals 1 with no rhythm difficulty, otherwise scales with a sqrt

            if (strainTimeBuffRange / avgDeltaTime > 1) // scale tap value for high BPM (above 200).
                strainValue += Math.Pow(strainTimeBuffRange / avgDeltaTime, 2);
            else
                strainValue += Math.Pow(strainTimeBuffRange / avgDeltaTime, 1);

            currentStrain *= computeDecay(baseDecay, osuCurrent.StrainTime);
            currentStrain += strainValue * strainMultiplier;

            return Math.Min(10 * strainValue * strainMultiplier, currentStrain * (Previous.Count / HistoryLength) * rhythmComplexity);
        }
    }
}
