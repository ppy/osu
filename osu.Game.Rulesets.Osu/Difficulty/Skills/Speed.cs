// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Objects;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : Skill
    {
        private double skillMultiplier => 0.95;

        private readonly List<double> noteDifficulties = new List<double>();

        private readonly List<double> sliderStrains = new List<double>();

        private double currentDifficulty;

        private double noteWeightSum;

        private double strainDecayBase => 0.3;

        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        public override void Process(DifficultyHitObject current)
        {
            currentDifficulty *= strainDecay(((OsuDifficultyHitObject)current).AdjustedDeltaTime);

            currentDifficulty += SpeedEvaluator.EvaluateDifficultyOf(current, Mods) * RhythmEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;

            if (current.BaseObject is Slider)
                sliderStrains.Add(currentDifficulty);

            noteDifficulties.Add(currentDifficulty);
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Notes with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These notes will not contribute to the difficulty.
            var peaks = noteDifficulties.Where(p => p > 0);

            List<double> notes = peaks.ToList();

            int index = 0;

            // Difficulty is the weighted sum of the highest notes.
            // We're sorting from highest to lowest note.
            foreach (double note in notes.OrderDescending())
            {
                // Use a harmonic sum which effectively buffs maps with more notes, especially if note difficulties are consistent.
                // Constants are arbitrary and give good values.
                // https://www.desmos.com/calculator/gquji01mlg
                double weight = (1.0 + (20.0 / (1 + index))) / (Math.Pow(index, 0.85) + 1.0 + (20.0 / (1.0 + index)));

                noteWeightSum += weight;

                difficulty += note * weight;
                index += 1;
            }

            return difficulty;
        }

        /// <summary>
        /// Returns the number of relevant objects weighted against the top note.
        /// </summary>
        public double CountTopWeightedNotes(double difficultyValue)
        {
            if (noteDifficulties.Count == 0)
                return 0.0;

            if (noteWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / noteWeightSum; // What would the top note be if all note values were identical

            if (consistentTopNote == 0)
                return 0;

            // Use a weighted sum of all notes. Constants are arbitrary and give nice values
            return noteDifficulties.Sum(s => 1.1 / (1 + Math.Exp(-5 * (s / consistentTopNote - 2))));
        }

        public double RelevantNoteCount()
        {
            if (noteDifficulties.Count == 0)
                return 0;

            double maxStrain = noteDifficulties.Max();
            if (maxStrain == 0)
                return 0;

            return noteDifficulties.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxStrain * 12.0 - 6.0))));
        }

        public double CountTopWeightedSliders(double difficultyValue)
        {
            if (sliderStrains.Count == 0)
                return 0;

            if (noteWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / noteWeightSum; // What would the top note be if all note values were identical

            if (consistentTopNote == 0)
                return 0;

            // Use a weighted sum of all notes. Constants are arbitrary and give nice values
            return sliderStrains.Sum(s => DifficultyCalculationUtils.Logistic(s / consistentTopNote, 3, 5, 1.1));
        }
    }
}
