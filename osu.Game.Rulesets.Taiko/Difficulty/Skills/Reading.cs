// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
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

        public Reading(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // Drum Rolls and Swells are exempt.
            if (current.BaseObject is not Hit)
            {
                return 0.0;
            }

            var taikoObject = (TaikoDifficultyHitObject)current;

            // Penalise repetitive patterns by decaying notes based on their index in the alternating mono pattern.
            int index = taikoObject.NoteIndex - (taikoObject.ColourData.AlternatingMonoPattern?.FirstHitObject.NoteIndex ?? taikoObject.NoteIndex);
            double simplePatternPenalty = DifficultyCalculationUtils.Logistic(index, 5, -1);

            currentStrain *= StrainDecayBase;
            currentStrain += ReadingEvaluator.EvaluateDifficultyOf(taikoObject) * SkillMultiplier * simplePatternPenalty;

            return currentStrain;
        }
    }
}
