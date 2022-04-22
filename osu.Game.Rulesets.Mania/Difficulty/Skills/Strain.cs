// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
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

        private readonly double[] holdEndTimes;
        private readonly double[] individualStrains;

        private double individualStrain;
        private double overallStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            holdEndTimes = new double[totalColumns];
            individualStrains = new double[totalColumns];
            overallStrain = 1;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double endTime = maniaCurrent.EndTime;
            int column = maniaCurrent.BaseObject.Column;
            double closestEndTime = Math.Abs(endTime - maniaCurrent.LastObject.StartTime); // Lowest value we can assume with the current information

            double holdFactor = 1.0; // Factor to all additional strains in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            // Fill up the holdEndTimes array
            for (int i = 0; i < holdEndTimes.Length; ++i)
            {
                // If there is at least one other overlapping end or note, then we get an addition
                if (Precision.DefinitelyBigger(holdEndTimes[i], maniaCurrent.StartTime, 1) && Precision.DefinitelyBigger(endTime, holdEndTimes[i], 1))
                    holdAddition = 1.0;

                // We give a slight bonus to everything if something is held meanwhile
                if (Precision.DefinitelyBigger(holdEndTimes[i], endTime, 1))
                    holdFactor = 1.25;

                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - holdEndTimes[i]));

                // Decay individual strains
                individualStrains[i] = applyDecay(individualStrains[i], current.DeltaTime, individual_decay_base);
            }

            holdEndTimes[column] = endTime;

            // The hold addition only is valid if there is _no_ other note with the same ending. Releasing multiple notes at the same time is just as easy as releasing 1
            // Nerfs the hold addition by half if the closest release is 24ms away
            if (holdAddition > 0)
                holdAddition *= 1 / (1 + Math.Exp(0.5 * (24 - closestEndTime)));

            // Increase individual strain in own column
            individualStrains[column] += 2.0 * holdFactor;
            individualStrain = individualStrains[column];

            overallStrain = applyDecay(overallStrain, current.DeltaTime, overall_decay_base) + (1 + holdAddition) * holdFactor;

            return individualStrain + overallStrain - CurrentStrain;
        }

        protected override double CalculateInitialStrain(double offset)
            => applyDecay(individualStrain, offset - Previous[0].StartTime, individual_decay_base)
               + applyDecay(overallStrain, offset - Previous[0].StartTime, overall_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
