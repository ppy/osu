// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Mods;
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

        private Mod[] mods;
        public readonly bool HiddenDifficultyOnly;

        public Reading(Mod[] mods, bool HiddenDifficultyOnly)
            : base(mods)
        {
            this.mods = mods;
            this.HiddenDifficultyOnly = HiddenDifficultyOnly;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Drum Rolls and Swells are exempt.
            if (current.BaseObject is not Hit)
            {
                return 0.0;
            }

            bool isHidden = mods.Any(m => m is TaikoModHidden);

            var taikoObject = (TaikoDifficultyHitObject)current;
            int index = taikoObject.ColourData.MonoStreak?.HitObjects.IndexOf(taikoObject) ?? 0;

            currentStrain *= DifficultyCalculationUtils.Logistic(index, 4, -1 / 25.0, 0.5) + 0.5;
            currentStrain *= StrainDecayBase;

            double difficulty = ReadingEvaluator.EvaluateDifficultyOf(taikoObject, mods, isHidden);

            if (HiddenDifficultyOnly)
            {
                double hiddenDifficulty = 0.0;

                if (isHidden)
                    hiddenDifficulty = difficulty - ReadingEvaluator.EvaluateDifficultyOf(taikoObject, mods, false);

                currentStrain += hiddenDifficulty * SkillMultiplier;
            }
            else
            {
                currentStrain += difficulty * SkillMultiplier;
            }

            return currentStrain;
        }
    }
}
