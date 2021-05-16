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
        private int averageLength = 4;
        private int globalCount;

        private int decayExcessThreshold = 500;

        private double currentStrain;
        private double strainMultiplier = 2.25;

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

        private double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count == 0)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double strainValue = .25;

            // double deltaTimeDeltaCount = 0;
            double sumDeltaTime = 0;

            if (Previous.Count < 8)
                return 0;

            for (int i = 4; i < Previous.Count; i++)
            {
                if (i - 4 < averageLength)
                    sumDeltaTime += ((OsuDifficultyHitObject)Previous[i]).StrainTime;
            }

            double avgDeltaTime = sumDeltaTime / Math.Min(Previous.Count, averageLength);

// {doubles, triplets, quads, quints, 6-tuplets, 7 Tuplets, greater}
            double[] islandSizes = {0, 0, 0, 0, 0, 0, 0};
            int islandSize = 0;
            double specialTransitionCount = 0;

            bool firstDeltaSwitch = false;
            bool lastCountedSwitch = false;

            for (int i = 1; i < Previous.Count; i++)
            {
                if (firstDeltaSwitch)
                {
                    if (Utils.IsRatioEqual(1.0, ((OsuDifficultyHitObject)Previous[i - 1]).StrainTime, ((OsuDifficultyHitObject)Previous[i]).StrainTime))
                    {
                        islandSize++;
                    }
                    else if (lastCountedSwitch)
                        lastCountedSwitch = false;
                    else if (Utils.IsRatioEqual(1.5, ((OsuDifficultyHitObject)Previous[i - 1]).StrainTime, ((OsuDifficultyHitObject)Previous[i]).StrainTime)
                              || Utils.IsRatioEqual(1.5, ((OsuDifficultyHitObject)Previous[i - 1]).StrainTime, ((OsuDifficultyHitObject)Previous[i]).StrainTime))
                    {
                        if (islandSize > 6)
                            islandSizes[6] = islandSizes[6] + 100.0 / ((OsuDifficultyHitObject)Previous[i]).StrainTime;
                        else
                            islandSizes[islandSize] = islandSizes[islandSize] + 100.0 / ((OsuDifficultyHitObject)Previous[i]).StrainTime;
                        islandSize = 0;
                        lastCountedSwitch = true;
                        if (Previous[i - 1].BaseObject is Slider || Previous[i].BaseObject is Slider)
                            specialTransitionCount += 100.0 / ((OsuDifficultyHitObject)Previous[i]).StrainTime;
                        else
                            specialTransitionCount += 200.0 / ((OsuDifficultyHitObject)Previous[i]).StrainTime;
                        specialTransitionCount++;
                    }
                    else
                    {
                        if (islandSize > 6)
                        {
                            islandSizes[6] = islandSizes[6] + 100 / ((OsuDifficultyHitObject)Previous[i]).StrainTime;
                        }
                        else
                            islandSizes[islandSize] = islandSizes[islandSize] + 1;
                        islandSize = 0;
                        lastCountedSwitch = true;
                    }
                }
                else if (!(Utils.IsRatioEqual(1.0, ((OsuDifficultyHitObject)Previous[i - 1]).StrainTime, ((OsuDifficultyHitObject)Previous[i]).StrainTime)))
                    firstDeltaSwitch = true;
            }

            double rhythmComplexitySum = 0.0;

            for (int i = 0; i < islandSizes.Length; i++)
            {
                rhythmComplexitySum += Math.Pow(islandSizes[i], .5);
            }

            // rhythmComplexitySum += Math.Pow(islandSizes[0], .5);
            // rhythmComplexitySum += Math.Pow(islandSizes[1] + islandSizes[3] + islandSizes[5], .5);
            // rhythmComplexitySum += Math.Pow(islandSizes[2] + islandSizes[4], .5);
            // rhythmComplexitySum += Math.Pow(islandSizes[6], .5);

            int sliderCount = 1;

            for (int i = 0; i < Previous.Count; i++)
            {
                if (Previous[i].BaseObject is Slider)
                    sliderCount++;
            }

            rhythmComplexitySum /= Math.Sqrt(9 + sliderCount) / 3;

            rhythmComplexitySum += specialTransitionCount;

            // rhythmComplexitySum *= 2;

            // for (int i = 0; i < Previous.Count; i++)
            // {
            //     if (i > 1 && Math.Abs(((OsuDifficultyHitObject)Previous[i - 1]).StrainTime - ((OsuDifficultyHitObject)Previous[i-2]).StrainTime) > 15)
            //         deltaTimeDeltaCount += 0.0;
            //     else if (i != 0 && (Math.Abs(((OsuDifficultyHitObject)Previous[i - 1]).StrainTime * 1.5 - ((OsuDifficultyHitObject)Previous[i]).StrainTime) < 15
            //                     || Math.Abs(((OsuDifficultyHitObject)Previous[i - 1]).StrainTime - 1.5 * ((OsuDifficultyHitObject)Previous[i]).StrainTime) < 15)
            //                && !(Previous[i - 1].BaseObject is Slider)
            //                && !(Previous[i].BaseObject is Slider))
            //         {
            //             if (Previous[i - 1].BaseObject is Slider)
            //                 deltaTimeDeltaCount += 1.5;
            //             else if (Previous[i].BaseObject is Slider)
            //                 deltaTimeDeltaCount += 0.75;
            //             else
            //                 deltaTimeDeltaCount += 5;
            //         }
            //     else if (i != 0 && Math.Abs(((OsuDifficultyHitObject)Previous[i - 1]).StrainTime - ((OsuDifficultyHitObject)Previous[i]).StrainTime) > 15
            //                && !(Previous[i - 1].BaseObject is Slider)
            //                && !(Previous[i].BaseObject is Slider))
            //         {
            //             if (Previous[i - 1].BaseObject is Slider)
            //                 deltaTimeDeltaCount += 1;
            //             else if (Previous[i].BaseObject is Slider)
            //                 deltaTimeDeltaCount += .5;
            //             else
            //                 deltaTimeDeltaCount += 2;
            //         }
            // }

            // Console.WriteLine("Delta: " + avgDeltaTime);

            if (75 / avgDeltaTime > 1)
                strainValue += Math.Pow(75 / avgDeltaTime, 2);
            else
                strainValue += 75 / avgDeltaTime;

            // strainValue += 1.5 / Math.Sqrt(hitWindowGreat);

//2.5 / Math.Sqrt(hitWindowGreat);
            // if (rhythmComplexitySum > 1)
            //     currentStrain *= (Previous.Count / HistoryLength) * Math.Sqrt(4 + rhythmComplexitySum) / 2;


            // Nerf by 12.5% for ultra stacks.
            // strainValue *= 1 - Math.Min(1, (osuCurrent.JumpDistance) / 50) / 8;

            currentStrain *= computeDecay(.9, osuCurrent.StrainTime);
            currentStrain += strainValue * strainMultiplier;


// Console.WriteLine("Count: " + globalCount);
// Console.WriteLine("Buff: " + Math.Sqrt((Previous.Count / HistoryLength) * Math.Sqrt(4 + rhythmComplexitySum) / 2));
// Console.WriteLine("Doubles: " + islandSizes[0] + " " + "Triples: " + islandSizes[1] + " " + "Quads: " +
//  islandSizes[2] + " " + "Quints: " + islandSizes[3] + " " + "Six: " + islandSizes[4] + " " + "Sevens: " + islandSizes[5] + " " + "Plus: " + islandSizes[6]);

            globalCount++;

            return currentStrain * (Previous.Count / HistoryLength) * Math.Max(1, Math.Sqrt(4 + rhythmComplexitySum) / 2);
        }

        public void SetHitWindow(double od, double clockRate)
        {
            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(od);

                    // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            hitWindowGreat = (int)(hitWindows.WindowFor(HitResult.Great)) / clockRate;
        }

        protected override void Process(DifficultyHitObject current)
        {
            double strain = strainValueAt(current);
            AddStrain(strain);
            // Console.WriteLine(hitWindowGreat);
        }
    }
}
