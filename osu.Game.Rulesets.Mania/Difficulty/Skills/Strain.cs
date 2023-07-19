// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainSkill
    {
        private const double individual_decay_base = 0.125;
        private const double overall_decay_base = 0.30;
        private const double release_threshold = 24;
        private readonly double[] endTimes;
        private readonly DecayingValue[] individualStrains;

        private DecayingValue currentIndividualStrain;
        private readonly DecayingValue overallStrain = DecayingValue.FromDecayMultiplierPerSecond(overall_decay_base);

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            endTimes = new double[totalColumns];
            individualStrains = new DecayingValue[totalColumns];

            for (int i = 0; i < totalColumns; i++)
            {
                individualStrains[i] = DecayingValue.FromDecayMultiplierPerSecond(individual_decay_base);
            }

            currentIndividualStrain = individualStrains[0];
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            var maniaCurrent = (ManiaDifficultyHitObject)current;
            double startTime = maniaCurrent.StartTime;
            double endTime = maniaCurrent.EndTime;
            int column = maniaCurrent.BaseObject.Column;
            bool isOverlapping = false;

            double closestEndTime = Math.Abs(endTime - startTime); // Lowest value we can assume with the current information
            double holdFactor = 1.0; // Factor to all additional strains in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            for (int i = 0; i < endTimes.Length; ++i)
            {
                // The current note is overlapped if a previous note or end is overlapping the current note body
                isOverlapping |= Precision.DefinitelyBigger(endTimes[i], startTime, 1) && Precision.DefinitelyBigger(endTime, endTimes[i], 1);

                // We give a slight bonus to everything if something is held meanwhile
                if (Precision.DefinitelyBigger(endTimes[i], endTime, 1))
                    holdFactor = 1.25;

                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - endTimes[i]));
            }

            // The hold addition is given if there was an overlap, however it is only valid if there are no other note with a similar ending.
            // Releasing multiple notes is just as easy as releasing 1. Nerfs the hold addition by half if the closest release is release_threshold away.
            // holdAddition
            //     ^
            // 1.0 + - - - - - -+-----------
            //     |           /
            // 0.5 + - - - - -/   Sigmoid Curve
            //     |         /|
            // 0.0 +--------+-+---------------> Release Difference / ms
            //         release_threshold
            if (isOverlapping)
                holdAddition = 1 / (1 + Math.Exp(0.5 * (release_threshold - closestEndTime)));

            // Decay and increase individualStrains in own column
            individualStrains[column].IncrementValueAtTime(startTime, 2.0 * holdFactor);

            // For notes at the same time (in a chord), the individualStrain should be the hardest individualStrain out of those columns
            if (maniaCurrent.DeltaTime > 1 || individualStrains[column].Value > currentIndividualStrain.Value)
                currentIndividualStrain = individualStrains[column];

            // slight hack to preserve old values - it's an insignificant difference that can be removed once the refactor is verified
            if (overallStrain.CurrentTime == 0)
            {
                overallStrain.UpdateTime(startTime - current.DeltaTime);
                overallStrain.Value = 1;
            }

            overallStrain.IncrementValueAtTime(startTime, (1 + holdAddition) * holdFactor);

            endTimes[column] = endTime;

            return currentIndividualStrain.Value + overallStrain.Value;
        }

        protected override double StrainAtTime(double time)
            => currentIndividualStrain.ValueAtTime(time)
               + overallStrain.ValueAtTime(time);
    }
}
