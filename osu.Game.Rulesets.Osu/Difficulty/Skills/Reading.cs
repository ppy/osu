// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Aggregation;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Reading : Skill
    {
        private readonly bool hasHiddenMod;
        private double harmonicWeightSum;

        public Reading(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.OfType<OsuModHidden>().Any(m => !m.OnlyFadeApproachCircles.Value);
        }

        private double currentStrain;

        private double reducedNoteCount;
        private double? reducedDuration;

        private double strainDecay(double ms) => DiffUtils.Pow(0.8, ms / 1000);

        protected override double ProcessInternal(DifficultyHitObject current)
        {
            const double skill_multiplier = 2.5;

            double decay = strainDecay(current.DeltaTime);

            currentStrain *= decay;
            currentStrain += calculateAdjustedDifficulty(current) * (1 - decay) * skill_multiplier;

            return currentStrain;
        }

        private double calculateAdjustedDifficulty(DifficultyHitObject current)
        {
            double difficulty = ReadingEvaluator.EvaluateDifficultyOf(current, hasHiddenMod);

            if (Mods.Any(m => m is OsuModTouchDevice))
                difficulty = DiffUtils.Pow(difficulty, 0.89);

            if (Mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = Mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                difficulty *= 1.0 - magnetisedStrength;
            }

            if (Mods.Any(m => m is OsuModRelax))
                difficulty *= 0.4;

            if (Mods.Any(m => m is OsuModAutopilot))
                difficulty *= 0.1;

            difficulty *= 0.825 + DiffUtils.Pow(Math.Max(0, ((OsuDifficultyHitObject)current).OverallDifficulty), 2.2) / 1125.0;

            return difficulty;
        }

        public override double CountTopWeightedObjectDifficulties(double difficultyValue)

        public override double DifficultyValue()
        {
            if (ObjectDifficulties.Count == 0)
                return 0;

            var difficulties = GetTransformedDifficulties(ObjectDifficulties);

            (double difficulty, harmonicWeightSum) = HarmonicSeries.Aggregate(difficulties);

            return difficulty;
        }

        protected override List<double> GetTransformedDifficulties(List<double> difficulties)
        {
            if (difficulties.Count == 0)
                return difficulties;

            const double early_reduced_difficulty_count = 200;
            const double early_reduced_difficulty_base_line = 0.5; // Assume the first objects are partially memorised

            for (int i = 0; i < Math.Min(early_reduced_difficulty_count, difficulties.Count); i++)
            {
                double ratio = i / early_reduced_difficulty_count;
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp(ratio, 0, 1)));
                difficulties[i] *= Interpolation.Lerp(early_reduced_difficulty_base_line, 1.0, scale);
            }

            const double memory_per_object = 0.04;
            const int maximum_sections_memorised = 100;
            const int minimum_sections_memorised = 10;
            const int reduction_window = 20;
            const double reduction_amount = 1.0 / 15;
            const double reduction_exponent = 0.8;

            int sectionsMemorised = (int)(memory_per_object * difficulties.Count);
            sectionsMemorised = Math.Clamp(sectionsMemorised + 5, minimum_sections_memorised, maximum_sections_memorised);

            for (int i = 0; i < sectionsMemorised; i++)
            {
                int index = 0;

                for (int j = 1; j < difficulties.Count; j++)
                {
                    if (difficulties[j] > difficulties[index])
                    {
                        index = j;
                    }
                }

                int lower = Math.Max(0, index - reduction_window);
                int upper = Math.Min(difficulties.Count - 1, index + reduction_window);
                double reductionFactor = 1 / DiffUtils.Pow(i + 1, reduction_exponent);

                for (int j = lower + 1; j < upper; j++)
                {
                    difficulties[j] *= 1 - ((reduction_window - Math.Abs(index - j)) * reduction_amount * reductionFactor / reduction_window);
                }
            }

            difficulties = difficulties.Where(v => v > 0).ToList();

            return difficulties;
        }

        public double CountTopWeightedObjectDifficulties(double difficultyValue)
        {
            if (ObjectDifficulties.Count == 0)
                return 0.0;

            if (harmonicWeightSum == 0)
                return 0.0;

            double consistentTopNote = difficultyValue / harmonicWeightSum; // What would the top difficulty be if all object difficulties were identical

            if (consistentTopNote == 0)
                return 0;

            return ObjectDifficulties.Sum(d => DiffUtils.Logistic(d / consistentTopNote, 1.15, 5, 1.1));
        }
    }
}
