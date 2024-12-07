// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the reading coefficient of taiko difficulty.
    /// </summary>
    public class Reading : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.4;

        private double currentStrain;

        public double ObjectDensity { get; private set; }

        public Reading(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current is not TaikoDifficultyHitObject taikoObject)
                return 0.0;

            // Drum Rolls and Swells are exempt.
            if (current.BaseObject is not Hit)
            {
                return 0.0;
            }

            ObjectDensity = ReadingEvaluator.CalculateObjectDensity(taikoObject) * 10;

            currentStrain *= StrainDecayBase;
            currentStrain += ReadingEvaluator.EvaluateDifficultyOf(taikoObject) * SkillMultiplier;

            return currentStrain;
        }
    }
}
