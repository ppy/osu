// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class LocalNoteDensity : StrainSkill
    {
        private double currentStrain;
        private double currentNoteCount;
        private double currentLongNoteWeight;

        private readonly List<double> activeKeyStrains;

        public LocalNoteDensity(Mod[] mods)
            : base(mods: mods)
        {
            currentNoteCount = 0;
            currentLongNoteWeight = 0;
            activeKeyStrains = new List<double>();
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => 0;

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            ManiaDifficultyHitObject maniaCurrent = (ManiaDifficultyHitObject)current;
            ManiaDifficultyHitObject prev = (ManiaDifficultyHitObject)current.Previous(0);

            currentNoteCount++;

            if (maniaCurrent.IsLong)
            {
                double longNoteDuration = Math.Min(maniaCurrent.EndTime - maniaCurrent.StartTime, 1000.0);
                currentLongNoteWeight += 0.5 * longNoteDuration / 200.0;
            }

            double currentTime = maniaCurrent.StartTime;
            double activeKeyValue = maniaCurrent.DifficultyContext.SampleFeatureAtTime(currentTime, maniaCurrent.DifficultyContext.ActiveKeyCount);
            activeKeyStrains.Add(activeKeyValue);

            if (prev != null && prev.StartTime == maniaCurrent.StartTime)
                return currentStrain;

            currentStrain = maniaCurrent.DifficultyContext.SampleFeatureAtTime(currentTime, maniaCurrent.DifficultyContext.LocalNoteCount);

            return currentStrain;
        }

        /*public override double DifficultyValue()
        {
            var peaks = GetCurrentStrainPeaks().Where(p => p > 0);
            if (!peaks.Any())
                return 0.0;

            return peaks.Average();
        }*/

        public List<double> GetActiveKeyStrains()
        {
            return activeKeyStrains;
        }

        public double GetTotalNotesWithWeight()
        {
            return currentNoteCount + currentLongNoteWeight;
        }
    }
}
