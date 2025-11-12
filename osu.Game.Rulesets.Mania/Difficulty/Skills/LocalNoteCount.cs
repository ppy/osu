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
    public class LocalNoteCount : StrainSkill
    {
        private const double strain_decay_base = .20143474157245744;
        private double currentStrain;
        private double currentNoteCount;
        private double currentLongNoteWeight;

        private readonly List<double> activeKeyStrains;

        public LocalNoteCount(Mod[] mods)
            : base(mods: mods)
        {
            currentNoteCount = 0;
            currentLongNoteWeight = 0;
            activeKeyStrains = new List<double>();
        }

        private double strainDecay(double ms) => Math.Pow(strain_decay_base, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current)
        {
            ManiaDifficultyHitObject prev = (ManiaDifficultyHitObject)current.Previous(0);
            double prevTime = prev.StartTime;
            double deltaMs = Math.Max(0.0, time - prevTime);
            return currentStrain * strainDecay(deltaMs);
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            ManiaDifficultyHitObject maniaCurrent = (ManiaDifficultyHitObject)current;
            ManiaDifficultyHitObject prev = (ManiaDifficultyHitObject)current.Previous(0);

            currentNoteCount++;

            if (maniaCurrent.EndTime > maniaCurrent.StartTime)
            {
                double longNoteDuration = Math.Min(maniaCurrent.EndTime - maniaCurrent.StartTime, 1000.0);
                currentLongNoteWeight += 0.5 * longNoteDuration / 200.0;
            }

            double currentTime = maniaCurrent.StartTime;
            double activeKeyValue = maniaCurrent.PreprocessedDifficultyData.SampleFeatureAtTime(currentTime, maniaCurrent.PreprocessedDifficultyData.ActiveKeyCount);
            activeKeyStrains.Add(activeKeyValue);

            if (prev != null && prev.StartTime == maniaCurrent.StartTime)
                return currentStrain;

            currentStrain = maniaCurrent.PreprocessedDifficultyData.SampleFeatureAtTime(currentTime, maniaCurrent.PreprocessedDifficultyData.LocalNoteCount);

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
