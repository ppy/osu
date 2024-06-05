// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public abstract class TouchSkill : OsuStrainSkill
    {
        private const int maximum_probabilities = 15;
        private readonly List<TouchProbability> probabilities = new List<TouchProbability>(maximum_probabilities);

        protected TouchSkill(Mod[] mods)
            : base(mods)
        {
        }

        protected double CalculateCurrentStrain(DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;

            if (current.Index == 0)
            {
                var probability = new TouchProbability(GetRawSkills());

                // Process the first object to add to history.
                probability.Process(osuCurrent, TouchHand.Drag);

                probabilities.Add(probability);

                return 0;
            }

            var newProbabilities = new List<TouchProbability>(probabilities.Count * 3);

            foreach (var probability in probabilities)
            {
                // Compute the probabilities of the object being hit by all possible hand movements.
                var leftProbability = new TouchProbability(probability);
                var rightProbability = new TouchProbability(probability);
                var dragProbability = new TouchProbability(probability);

                leftProbability.Process(osuCurrent, TouchHand.Left);
                rightProbability.Process(osuCurrent, TouchHand.Right);
                dragProbability.Process(osuCurrent, TouchHand.Drag);

                double leftStrain = GetProbabilityTotalStrain(leftProbability);
                double rightStrain = GetProbabilityTotalStrain(rightProbability);
                double dragStrain = GetProbabilityTotalStrain(dragProbability);

                // Apply a weighted sum to determine the hand that is the most likely to hit the object.
                double leftWeight = Math.Sqrt(rightStrain * dragStrain);
                double rightWeight = Math.Sqrt(leftStrain * dragStrain);
                double dragWeight = Math.Sqrt(leftStrain * rightStrain);
                double sumWeight = leftWeight + rightWeight + dragWeight;

                leftProbability.Probability *= sumWeight > 0 ? leftWeight / sumWeight : 1.0 / 3;
                rightProbability.Probability *= sumWeight > 0 ? rightWeight / sumWeight : 1.0 / 3;
                dragProbability.Probability *= sumWeight > 0 ? dragWeight / sumWeight : 1.0 / 3;

                newProbabilities.Add(leftProbability);
                newProbabilities.Add(rightProbability);
                newProbabilities.Add(dragProbability);
            }

            // Only keep the most probable possibilities.
            var sortedProbabilities = newProbabilities.OrderByDescending(p => p.Probability).ToList();
            probabilities.Clear();

            double totalMostProbable = 0;

            for (int i = 0; i < Math.Min(sortedProbabilities.Count, maximum_probabilities); i++)
            {
                totalMostProbable += sortedProbabilities[i].Probability;

                probabilities.Add(sortedProbabilities[i]);
            }

            double strain = 0;

            foreach (var probability in probabilities)
            {
                // Make sure total probability sums up to 1.
                probability.Probability = totalMostProbable > 0 ? probability.Probability / totalMostProbable : 1.0 / probabilities.Count;

                strain += GetProbabilityStrain(probability) * probability.Probability;
            }

            return strain;
        }

        /// <summary>
        /// Evaluates the total strain of this <see cref="TouchSkill"/>.
        /// </summary>
        /// <param name="aimStrain">The aim strain represented by this <see cref="TouchSkill"/>.</param>
        /// <param name="speedStrain">The speed strain represented by this <see cref="TouchSkill"/>.</param>
        /// <returns>The total strain.</returns>
        protected double CalculateTotalStrain(double aimStrain, double speedStrain) => Math.Pow(Math.Pow(aimStrain, 1.5) + Math.Pow(speedStrain, 1.5), 2.0 / 3);

        /// <summary>
        /// Obtains the <see cref="RawTouchSkill"/>s to be computed by this <see cref="TouchSkill"/>.
        /// </summary>
        /// <returns>The <see cref="RawTouchSkill"/>s to compute.</returns>
        protected abstract RawTouchSkill[] GetRawSkills();

        /// <summary>
        /// Evaluates the strain of a <see cref="TouchProbability"/> that is specific to the skillset that this <see cref="TouchSkill"/> represents.
        /// </summary>
        /// <param name="probability">The <see cref="TouchProbability"/> to compute for.</param>
        /// <returns>The strain of the <see cref="TouchProbability"/> that is specific to the skillset that this <see cref="TouchSkill"/> represents.</returns>
        protected abstract double GetProbabilityStrain(TouchProbability probability);

        /// <summary>
        /// Evaluates the strain of a <see cref="TouchProbability"/> that applies all necessary <see cref="RawTouchSkill"/>s.
        /// </summary>
        /// <param name="probability">The <see cref="TouchProbability"/> to compute for.</param>
        /// <returns>The strain of the <see cref="TouchProbability"/> that applies all necessary <see cref="RawTouchSkill"/>s.</returns>
        protected abstract double GetProbabilityTotalStrain(TouchProbability probability);
    }
}
