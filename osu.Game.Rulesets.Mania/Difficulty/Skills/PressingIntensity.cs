// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class PressingIntensity : StrainSkill
    {
        private const double strain_decay_base = .20143474157245744;

        private readonly SunnyStrainData strainData;
        private double currentStrain;

        public PressingIntensity(Mod[] mods, SunnyStrainData data)
            : base(mods: mods)
        {
            strainData = data;
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

            if (prev != null && prev.StartTime == maniaCurrent.StartTime)
                return currentStrain;

            double currentTime = maniaCurrent.StartTime;
            currentStrain = PressingIntensityEvaluator.EvaluateDifficultyAt(currentTime, strainData);
            return currentStrain;
        }
    }
}
