// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : Skill
    {
        private readonly List<double> noteDifficulties = new List<double>();

        private readonly List<double> noteWeights = new List<double>();

        private readonly IReadOnlyList<HitObject> objectList;

        private readonly double clockRate;
        private readonly bool hasHiddenMod;
        private readonly double preempt;
        private double skillMultiplier => 23.0;

        public Reading(IBeatmap beatmap, Mod[] mods, double clockRate)
            : base(mods)
        {
            this.clockRate = clockRate;
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
            preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) / clockRate;
            objectList = beatmap.HitObjects;
        }

        public static double DifficultyToPerformance(double difficulty) => 25 * Math.Pow(difficulty, 2);

        public override void Process(DifficultyHitObject current) => noteDifficulties.Add(ReadingEvaluator.EvaluateDifficultyOf(objectList.Count, current, clockRate, preempt, hasHiddenMod) * skillMultiplier);

        public override double DifficultyValue()
        {
            double difficulty = 0;

            // Notes with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These notes will not contribute to the difficulty.
            var peaks = noteDifficulties.Where(p => p > 0);

            List<double> notes = peaks.ToList();

            const double reduced_duration = 60 * 1000.0;

            const double reduced_base_line = 0.0; // Assume the first seconds are completely memorised

            double reducedNoteCount = 0;

            foreach (var hitObject in objectList)
            {
                if (hitObject.StartTime / clockRate > reduced_duration)
                    break;

                reducedNoteCount++;
            }

            for (int i = 0; i < Math.Min(notes.Count, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((float)i / reducedNoteCount, 0, 1)));
                double mult = Interpolation.Lerp(reduced_base_line, 1.0, scale);
                notes[i] *= mult;
            }

            int index = 0;

            // Difficulty is the weighted sum of the highest notes.
            // We're sorting from highest to lowest note.
            foreach (double note in notes.OrderDescending())
            {
                // Use a harmonic sum for note which effectively buffs maps with more notes, especially if note difficulties are consistent.
                // Constants are arbitrary and give good values.
                // https://www.desmos.com/calculator/gquji01mlg
                double weight = (1.0 + (1.0 / (1 + index))) / (Math.Pow(index, 0.8) + 1.0 + (1.0 / (1.0 + index)));

                noteWeights.Add(weight);

                difficulty += note * weight;
                index += 1;
            }

            return difficulty;
        }

        /// <summary>
        /// Returns the number of relevant objects weighted against the top note.
        /// </summary>
        public virtual double CountTopWeightedNotes()
        {
            if (noteDifficulties.Count == 0)
                return 0.0;

            double consistentTopNote = DifficultyValue() / noteWeights.Sum(); // What would the top note be if all note values were identical

            if (noteWeights.Sum() == 0)
                return 0.0;

            if (consistentTopNote == 0)
                return noteDifficulties.Count;

            // Use a weighted sum of all notes. Constants are arbitrary and give nice values
            return noteDifficulties.Sum(s => 1.1 / (1 + Math.Exp(-10 * (s / consistentTopNote - 0.88))));
        }
    }
}
