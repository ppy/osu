// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainDecaySkill
    {
        private const double individual_decay_base = 0.125;
        private const double overall_decay_base = 0.30;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        private readonly double[] individualStrains;
        private double highestIndividualStrain;
        private double overallStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            individualStrains = new double[totalColumns];
            overallStrain = 1;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;

            individualStrains[maniaCurrent.Column] = applyDecay(individualStrains[maniaCurrent.Column], maniaCurrent.ColumnStrainTime, individual_decay_base);
            individualStrains[maniaCurrent.Column] += IndividualStrainEvaluator.EvaluateDifficultyOf(current);

            // Take the hardest individualStrain for notes that happen at the same time (in a chord).
            // This is to ensure the order in which the notes are processed does not affect the resultant total strain.
            highestIndividualStrain = maniaCurrent.DeltaTime <= 1 ? Math.Max(highestIndividualStrain, individualStrains[maniaCurrent.Column]) : individualStrains[maniaCurrent.Column];

            overallStrain = applyDecay(overallStrain, maniaCurrent.DeltaTime, overall_decay_base);
            overallStrain += OverallStrainEvaluator.EvaluateDifficultyOf(current);

            // By subtracting CurrentStrain, this skill effectively only considers the maximum strain of any one hitobject within each strain section.
            return highestIndividualStrain + overallStrain - CurrentStrain;
        }

        protected override double CalculateInitialStrain(double offset, DifficultyHitObject current) =>
            applyDecay(highestIndividualStrain, offset - current.Previous(0).StartTime, individual_decay_base)
            + applyDecay(overallStrain, offset - current.Previous(0).StartTime, overall_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
