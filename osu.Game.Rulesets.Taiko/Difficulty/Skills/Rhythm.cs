// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class RhythmAttributes : StrainSkillAttributes;

    /// <summary>
    /// Calculates the rhythm coefficient of taiko difficulty.
    /// </summary>
    public class Rhythm : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1.0;
        protected override double StrainDecayBase => 0.4;

        public Rhythm(Mod[] mods, DifficultyHitObject[] difficultyHitObjects)
            : base(mods, difficultyHitObjects)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double difficulty = RhythmEvaluator.EvaluateDifficultyOf(current);

            // To prevent abuse of exceedingly long intervals between awkward rhythms, we penalise its difficulty.
            double staminaDifficulty = StaminaEvaluator.EvaluateDifficultyOf(current) - 0.5; // Remove base strain
            difficulty *= DiffUtils.Logistic(staminaDifficulty, 1 / 15.0, 50.0);

            return difficulty;
        }

        public override ISkillAttributes Process()
        {
            var baseAttributes = (StrainSkillAttributes)base.Process();

            return new RhythmAttributes
            {
                Difficulty = baseAttributes.Difficulty,
                ObjectDifficulties = baseAttributes.ObjectDifficulties,
                StrainPeaks = baseAttributes.StrainPeaks,
                TopWeightedStrainsCount = baseAttributes.TopWeightedStrainsCount
            };
        }

        public override IEnumerable<TimedSkillAttributes> ProcessTimed()
        {
            foreach (var baseTimedAttributes in base.ProcessTimed())
            {
                var baseAttributes = (StrainSkillAttributes)baseTimedAttributes.Attributes;

                yield return new TimedSkillAttributes(new RhythmAttributes
                {
                    Difficulty = baseAttributes.Difficulty,
                    ObjectDifficulties = baseAttributes.ObjectDifficulties,
                    StrainPeaks = baseAttributes.StrainPeaks,
                    TopWeightedStrainsCount = baseAttributes.TopWeightedStrainsCount
                }, baseTimedAttributes.Time);
            }
        }
    }
}
