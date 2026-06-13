// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class HarmonicSkill : Skill
    {
        /// <summary>
        /// The sum of note weights, calculated during summation.
        /// Required for any calculations which need to normalise difficulty value.
        /// </summary>
        protected double NoteWeightSum;

        /// <summary>
        /// Scaling factor applied as HarmonicScale / (1 + index) during weight calculations.
        /// A higher value will increase the influence of the hardest object difficulties during summation.
        /// </summary>
        protected virtual double HarmonicScale => 1.0;

        /// <summary>
        /// Exponent that controls the rate of which decay increases as the index increases.
        /// Values closer to 1 decay faster whilst lower values give more weight to lower object difficulties.
        /// </summary>
        protected virtual double DecayExponent => 0.9;

        protected HarmonicSkill(Mod[] mods)
            : base(mods)
        {
        }

        /// <summary>
        /// Returns the difficulty value of the current <see cref="DifficultyHitObject"/>. This value is calculated with or without respect to previous objects.
        /// </summary>
        protected abstract double ObjectDifficultyOf(DifficultyHitObject current);

        protected sealed override double ProcessInternal(DifficultyHitObject current)
            => ObjectDifficultyOf(current);

        /// <summary>
        /// Transforms the object difficulties specifically for final difficulty summation.
        /// This can be used to decrease weight of certain notes based on a skill-specific criteria.
        /// </summary>
        protected virtual void ApplyDifficultyTransformation(double[] difficulties)
        {
        }

        public override double DifficultyValue()
        {
            if (ObjectDifficulties.Count == 0)
                return 0;

            // Notes with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These notes will not contribute to the difficulty.
            double[] difficulties = ObjectDifficulties.Where(p => p > 0).ToArray();

            if (difficulties.Length == 0)
                return 0;

            ApplyDifficultyTransformation(difficulties);

            double difficulty = 0;
            int index = 0;

            foreach (double note in difficulties.OrderDescending())
            {
                // Use a harmonic sum that considers each note of the map according to a predefined weight.
                double weight = (1 + (HarmonicScale / (1 + index))) / (Math.Pow(index, DecayExponent) + 1 + (HarmonicScale / (1 + index)));

                NoteWeightSum += weight;

                difficulty += note * weight;
                index += 1;
            }

            return difficulty;
        }

        /// <summary>
        /// Calculates the number of object difficulties weighted against the top object difficulty.
        /// </summary>
        public virtual double CountTopWeightedObjectDifficulties(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            if (NoteWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / NoteWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopNote == 0)
                return 0;

            return ObjectDifficulties.Sum(d => DifficultyCalculationUtils.Logistic(d / consistentTopNote, 0.88, 10, 1.1));
        }

        public static double DifficultyToPerformance(double difficulty) => 4.0 * Math.Pow(difficulty, 3.0);
    }
}
