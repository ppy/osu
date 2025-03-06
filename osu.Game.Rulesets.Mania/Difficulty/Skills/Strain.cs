// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainSkill
    {
        private const double individual_decay_base = 0.125;
        private const double overall_decay_base = 0.30;

        private readonly double[] startTimes;
        private readonly double[] endTimes;
        private readonly double[] individualStrains;

        private double overallStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            startTimes = new double[totalColumns];
            endTimes = new double[totalColumns];
            individualStrains = new double[totalColumns];
            overallStrain = 1;
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            var previousNotes = maniaCurrent.PreviousHitObjects;

            double individualStrain = individualStrains[maniaCurrent.Column];

            // Decay and increase individualStrains in own column
            individualStrain = applyDecay(individualStrain, maniaCurrent.ColumnStrainTime ?? double.PositiveInfinity, individual_decay_base);
            individualStrain += StrainEvaluator.EvaluateDifficultyOf(current);

            // When all previous chord notes are populated, increase overallStrain, then decay the time since previous chord.
            if (current.DeltaTime > 0)
            {
                for (int i = 0; i < individualStrains.Length; i++)
                {
                    double decayedIndividualStrain = applyDecay(individualStrains[i], current.StartTime - current.DeltaTime - previousNotes[i]?.StartTime ?? double.PositiveInfinity, overall_decay_base);

                    overallStrain = DifficultyCalculationUtils.Norm(4, overallStrain, decayedIndividualStrain);
                }
            }

            overallStrain = applyDecay(overallStrain, current.DeltaTime, overall_decay_base);

            individualStrains[maniaCurrent.Column] = individualStrain;

            // By subtracting CurrentStrain, this skill effectively only considers the maximum strain of any one hitobject within each strain section.
            return individualStrain + overallStrain;
        }

        protected override double CalculateInitialStrain(double offset, DifficultyHitObject current) => applyDecay(overallStrain, offset - current.Previous(0).StartTime, overall_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
