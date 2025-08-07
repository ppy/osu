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
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : Skill
    {
        private readonly List<double> noteDifficulties = new List<double>();
        private readonly IReadOnlyList<HitObject> objectList;

        private readonly double clockRate;
        private readonly bool hasHiddenMod;
        private readonly double preempt;
        private double skillMultiplier => 2.0;

        public Reading(IBeatmap beatmap, Mod[] mods, double clockRate)
            : base(mods)
        {
            this.clockRate = clockRate;
            hasHiddenMod = mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value);
            preempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, OsuHitObject.PREEMPT_MAX, OsuHitObject.PREEMPT_MID, OsuHitObject.PREEMPT_MIN) / clockRate;
            objectList = beatmap.HitObjects;
        }

        private double currentDifficulty;
        private double noteWeightSum;
        private double strainDecayBase => 0.8;

        public static double DifficultyToPerformance(double difficulty) => 25 * Math.Pow(difficulty, 2);
        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        public override void Process(DifficultyHitObject current)
        {
            currentDifficulty *= strainDecay(current.DeltaTime);

            currentDifficulty += ReadingEvaluator.EvaluateDifficultyOf(objectList.Count, current, clockRate, preempt, hasHiddenMod) * skillMultiplier;

            noteDifficulties.Add(currentDifficulty);
        }

        public override double DifficultyValue()
        {
            if (objectList.Count == 0)
                return 0;

            double difficulty = 0;

            // Notes with 0 difficulty are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
            // These notes will not contribute to the difficulty.
            var peaks = noteDifficulties.Where(p => p > 0);

            List<double> notes = peaks.ToList();

            double reducedDuration = objectList[0].StartTime / clockRate + 60 * 1000.0; // Start time at first object

            const double reduced_base_line = 0.0; // Assume the first seconds are completely memorised

            double reducedNoteCount = 0;

            foreach (var hitObject in objectList)
            {
                if (hitObject.StartTime / clockRate > reducedDuration)
                    break;

                reducedNoteCount++;
            }

            for (int i = 0; i < Math.Min(notes.Count, reducedNoteCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp(i / reducedNoteCount, 0, 1)));
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
                // https://www.desmos.com/calculator/5eb60faf4c
                double weight = (1.0 + (1.0 / (1 + index))) / (Math.Pow(index, 0.8) + 1.0 + (1.0 / (1.0 + index)));

                noteWeightSum += weight;

                difficulty += note * weight;
                index += 1;
            }

            return difficulty;
        }

        /// <summary>
        /// Returns the number of relevant objects weighted against the top note.
        /// </summary>
        public double CountTopWeightedNotes()
        {
            if (noteDifficulties.Count == 0)
                return 0.0;

            double consistentTopNote = DifficultyValue() / noteWeightSum; // What would the top note be if all note values were identical

            if (noteWeightSum == 0)
                return 0.0;

            if (consistentTopNote == 0)
                return 0;

            // Use a weighted sum of all notes. Constants are arbitrary and give nice values
            return noteDifficulties.Sum(s => 1.1 / (1 + Math.Exp(-5 * (s / consistentTopNote - 1.15))));
        }
    }
}
