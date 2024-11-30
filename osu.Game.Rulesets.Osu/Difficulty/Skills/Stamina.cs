// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Stamina : StrainSkill
    {
        private double skillMultiplier => 0.0008;
        private double strainDecayBase => 0.99;

        private double currentStrain;

        public Stamina(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);
            currentStrain += EvaluateDifficultyOf(current) * skillMultiplier;

            double totalStrain = currentStrain;

            return totalStrain;
        }

        private const double speed_balancing_factor = 40;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            // derive strainTime for calculation
            var osuCurrObj = (OsuDifficultyHitObject)current;

            double strainTime = osuCurrObj.StrainTime;
            double speedBonus = 0.0;

            if (DifficultyCalculationUtils.MillisecondsToBPM(strainTime) > 220)
                speedBonus = Math.Pow((DifficultyCalculationUtils.BPMToMilliseconds(220) - strainTime) / 40, 2);

            return (1 + speedBonus) * 1000 / strainTime;
        }
    }
}
