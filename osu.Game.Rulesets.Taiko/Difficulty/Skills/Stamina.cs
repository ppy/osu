// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the stamina coefficient of taiko difficulty.
    /// </summary>
    public class Stamina : StrainSkill
    {
        private double skillMultiplier => 1.1;
        private double strainDecayBase => 0.4;

        public readonly bool SingleColourStamina;
        private readonly bool isConvert;

        private double currentStrain;

        /// <summary>
        /// Creates a <see cref="Stamina"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        /// <param name="singleColourStamina">Reads when Stamina is from a single coloured pattern.</param>
        /// <param name="isConvert">Determines if the currently evaluated beatmap is converted.</param>
        public Stamina(Mod[] mods, bool singleColourStamina, bool isConvert)
            : base(mods)
        {
            SingleColourStamina = singleColourStamina;
            this.isConvert = isConvert;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += StaminaEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;

            // Safely prevents previous strains from shifting as new notes are added.
            var currentObject = current as TaikoDifficultyHitObject;
            int index = currentObject?.ColourData.MonoStreak?.HitObjects.IndexOf(currentObject) ?? 0;

            double monolengthBonus = isConvert ? 1 : 1 + Math.Min(Math.Max((index - 5) / 50.0, 0), 0.30);

            if (SingleColourStamina)
                return DifficultyCalculationUtils.Logistic(-(index - 10) / 2.0, currentStrain);

            return currentStrain * monolengthBonus;
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => SingleColourStamina ? 0 : currentStrain * strainDecay(time - current.Previous(0).StartTime);
    }
}
