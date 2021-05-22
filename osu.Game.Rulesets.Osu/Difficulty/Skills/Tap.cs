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
        private int averageLength = 2;
        private int globalCount;

        private int decayExcessThreshold = 500;

        private double currentStrain;
        private double strainMultiplier = 2.675;//1.5;//3125;

        private double hitWindowGreat;

        public Tap(Mod[] mods)
            : base(mods)
        {
        }

        public double TapStrain => currentStrain;

        private double computeDecay(double baseDecay, double ms)
        {
            double decay = 0;
            if (ms < decayExcessThreshold)
                decay = baseDecay;
            else
                decay = Math.Pow(Math.Pow(baseDecay, 1000 / Math.Min(ms, decayExcessThreshold)), ms / 1000);

            return decay;
        }

        protected override double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count == 0)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double strainValue = .25;

            // double deltaTimeDeltaCount = 0;
            double sumDeltaTime = 0;

            if (Previous.Count < 8)
                return 0;

            for (int i = 0; i < Previous.Count; i++)
            {
                if (i < averageLength)
                    sumDeltaTime += ((OsuDifficultyHitObject)Previous[i]).StrainTime;
            }

            double avgDeltaTime = sumDeltaTime / Math.Min(Previous.Count, averageLength);

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

                if (Utils.IsRatioEqual(1.5, prevDelta, currDelta) || Utils.IsRatioEqual(1.5, currDelta, prevDelta))
                {
                    if (Previous[i - 1].BaseObject is Slider || Previous[i].BaseObject is Slider)
                        specialTransitionCount += 50.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                    else
                        specialTransitionCount += 250.0 / Math.Sqrt(prevDelta * currDelta) * ((double)i / HistoryLength);
                }

                if (firstDeltaSwitch)
                {
                    if (Utils.IsRatioEqual(1.0, prevDelta, currDelta))
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
                    rhythmComplexitySum += islandTimes[i] / Math.Pow(islandSizes[i], .5);
            }

            // rhythmComplexitySum += islandTimes[0] / Math.Pow(islandSizes[0], .75);
            // rhythmComplexitySum += islandTimes[2] / Math.Pow(islandSizes[2], .75);
            // rhythmComplexitySum += islandTimes[4] / Math.Pow(islandSizes[4], .75);
            // rhythmComplexitySum += islandTimes[5] / Math.Pow(islandSizes[5], .75);
            // rhythmComplexitySum += (islandSizes[1] + islandSizes[3]) / Math.Pow(islandSizes[1] + islandSizes[3], .75);
            // rhythmComplexitySum += islandTimes[6] / Math.Pow(islandSizes[6], .75);

            int sliderCount = 1;

            for (int i = 0; i < Previous.Count; i++)
            {
                if (Previous[i].BaseObject is Slider)
                    sliderCount++;
            }

            // rhythmComplexitySum /= Math.Sqrt(9 + sliderCount) / 3;


            rhythmComplexitySum += specialTransitionCount;
            rhythmComplexitySum *= .75;
            // Console.WriteLine("Delta: " + avgDeltaTime);

            if (75 / avgDeltaTime > 1)
                strainValue += Math.Pow(75 / avgDeltaTime, 2);
            else
                strainValue += Math.Pow(75 / avgDeltaTime, 1);

            // strainValue += 1.5 / Math.Sqrt(hitWindowGreat);

//2.5 / Math.Sqrt(hitWindowGreat)

            // Nerf by 12.5% for ultra stacks.
            // strainValue *= 1 - Math.Min(1, (osuCurrent.JumpDistance) / 50) / 8;

            currentStrain *= computeDecay(.9, osuCurrent.StrainTime);
            currentStrain += strainValue * strainMultiplier;


// Console.WriteLine("Count: " + globalCount);
// Console.WriteLine("Buff: " + Math.Sqrt((Previous.Count / HistoryLength) * Math.Sqrt(4 + rhythmComplexitySum) / 2));
// Console.WriteLine("Doubles: " + islandSizes[0] + " " + "Triples: " + islandSizes[1] + " " + "Quads: " +
//  islandSizes[2] + " " + "Quints: " + islandSizes[3] + " " + "Six: " + islandSizes[4] + " " + "Sevens: " + islandSizes[5] + " " + "Plus: " + islandSizes[6]);

            globalCount++;
            // Console.WriteLine(hitWindowGreat);

            // if (rhythmComplexitySum > 1)
                return currentStrain * (Previous.Count / HistoryLength) * (Math.Sqrt(4 + rhythmComplexitySum) / 2);


            // else
            //     return currentStrain;// * (Previous.Count / HistoryLength) * Math.Max(1, Math.Sqrt(3 + rhythmComplexitySum) / 2);
        }

        public void SetHitWindow(double od, double clockRate)
        {
            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(od);

                    // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            hitWindowGreat = (int)(hitWindows.WindowFor(HitResult.Great)) / clockRate;
        }
    }
}
