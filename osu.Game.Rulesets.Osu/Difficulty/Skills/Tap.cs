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

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Tap : OsuSkill
    {
        protected override double StarsPerDouble => 1.075;
        protected override int HistoryLength => 8;
        private int averageLength = 4;

        private int decayExcessThreshold = 500;

        private double currentStrain;
        private double strainMultiplier = 4.75;

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

            int deltaTimeDeltaCount = 0;
            double sumDeltaTime = 0;

            for (int i = 0; i < Previous.Count; i++)
            {
                if (i != 0 && Math.Abs(((OsuDifficultyHitObject)Previous[i - 1]).StrainTime - ((OsuDifficultyHitObject)Previous[i]).StrainTime) > 15)
                    deltaTimeDeltaCount++;
                if (i < averageLength)
                    sumDeltaTime += ((OsuDifficultyHitObject)Previous[i]).StrainTime;
            }

            double avgDeltaTime = sumDeltaTime / Math.Min(Previous.Count, averageLength);

            // Console.WriteLine("Delta: " + avgDeltaTime);

            if (75 / avgDeltaTime > 1)
                strainValue += Math.Pow(75 / avgDeltaTime, 2);
            else
                strainValue += 75 / avgDeltaTime;

            // strainValue += 1.5 / Math.Sqrt(hitWindowGreat);

//2.5 / Math.Sqrt(hitWindowGreat);
            // strainValue *= 1.75;//* (Previous.Count / HistoryLength) * Math.Sqrt(9 + deltaTimeDeltaCount) / 3;

            currentStrain *= computeDecay(.85, osuCurrent.StrainTime);
            currentStrain += strainValue * strainMultiplier;


// Console.WriteLine("Strain: " + currentStrain);

            return currentStrain;
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
            currentStrain = strainValueAt(current);
            AddStrain(currentStrain);
            // Console.WriteLine(hitWindowGreat);
        }
    }
}
