// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainDecaySkill
    {
        /// <summary> Column Strain Decay Exponent Base. Used in <see cref="applyDecay"/> </summary>
        private const double column_decay_base = 0.125;

        /// <summary> Global Strain Decay Exponent Base. Used in <see cref="applyDecay"/> </summary>
        private const double global_decay_base = 0.30;

        /// <summary> Center of our endOnBodyBias sigmoid function. </summary>
        private const double release_threshold = 24;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        /// <summary> Previous notes' start times. Indices correspond to columns </summary>
        private readonly double[] prevStartTimes;

        /// <summary> Previous notes' end times. Indices correspond to columns </summary>
        private readonly double[] prevEndTimes;

        /// <summary> Previous Strain processed. Used in Maximizing Strain Summation for Deterministic Chord Strains </summary>
        private double prevColumnStrain;

        private readonly double[] columnStrains;
        private double globalStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            prevStartTimes = new double[totalColumns];
            prevEndTimes = new double[totalColumns];
            columnStrains = new double[totalColumns];
            globalStrain = 1;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        ///
        /// <remarks>
        /// The function documentation references the README.md in the same directory.
        /// </remarks>
        /// </summary>
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var hitObject = (ManiaDifficultyHitObject)current;

            // Given a note, startTime == endTime.
            double startTime = hitObject.StartTime;
            double endTime = hitObject.EndTime;
            int column = hitObject.BaseObject.Column;
            double holdLength = Math.Abs(endTime - startTime);

            // See README Section: LN Strain Bonus Triggers
            double endOnBodyBias = 0; // Strain Bias for Column 3 states
            double endAfterTailWeight = 1.0; // Strain Weight for Column 5 states
            bool isEndOnBody = false; // Flag for Column 3 states
            bool isEndAfterTail = false; // Flag for Column 5 states

            // The closest end time, currently, is the current note's end time, which is its length
            // This is used for endOnBodyBias calculation.
            double closestEndTime = holdLength;

            for (int i = 0; i < prevEndTimes.Length; ++i)
            {
                isEndOnBody |= // Accepts Col 3 States
                    prevEndTimes[i] - 1 > startTime && // Accepts Col 3-5
                    prevEndTimes[i] < endTime - 1; // Accepts Col 1 & D2:F3

                // Accepts Col 5 States
                isEndAfterTail |= prevEndTimes[i] - 1 > endTime;

                // Update closest end time by looking through previous LNs
                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - prevEndTimes[i]));
            }

            if (isEndOnBody)
                endOnBodyBias = 1 / (1 + Math.Exp(0.5 * (release_threshold - closestEndTime)));

            if (isEndAfterTail)
                endAfterTailWeight = 1.25;

            // Update Column & Global Strains
            columnStrains[column] = applyDecay(columnStrains[column], startTime - prevStartTimes[column], column_decay_base);
            columnStrains[column] += 2 * endAfterTailWeight;
            globalStrain = applyDecay(globalStrain, current.DeltaTime, global_decay_base);
            globalStrain += (1 + endOnBodyBias) * endAfterTailWeight;

            // See README Section: Maximizing Strain Summation for Deterministic Chord Strains
            double columnStrain = hitObject.DeltaTime <= 1 ? Math.Max(prevColumnStrain, columnStrains[column]) : columnStrains[column];

            double strain = columnStrain + globalStrain;

            // Update prev arrays
            prevStartTimes[column] = startTime;
            prevEndTimes[column] = endTime;
            prevColumnStrain = columnStrain;

            // We substract CurrentStrain, because we add back the CurrentStrain on the outside function.
            return strain - CurrentStrain;
        }

        protected override double CalculateInitialStrain(double offset, DifficultyHitObject current)
            => applyDecay(prevColumnStrain, offset - current.Previous(0).StartTime, column_decay_base)
               + applyDecay(globalStrain, offset - current.Previous(0).StartTime, global_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
