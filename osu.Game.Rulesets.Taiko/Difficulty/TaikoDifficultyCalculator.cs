// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.084375;
        private const double rhythm_skill_multiplier = 0.750 * difficulty_multiplier;
        private const double reading_skill_multiplier = 0.100 * difficulty_multiplier;
        private const double colour_skill_multiplier = 0.375 * difficulty_multiplier;
        private const double stamina_skill_multiplier = 0.445 * difficulty_multiplier;

        private double strainLengthBonus;
        private double patternMultiplier;

        private bool isRelax;
        private bool isConvert;

        public override int Version => 20251020;

        public TaikoDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            isConvert = beatmap.BeatmapInfo.Ruleset.OnlineID == 0;
            isRelax = mods.Any(h => h is TaikoModRelax);

            return new Skill[]
            {
                new Rhythm(mods, hitWindows.WindowFor(HitResult.Great) / clockRate),
                new Reading(mods),
                new Colour(mods),
                new Stamina(mods, false, isConvert),
                new Stamina(mods, true, isConvert)
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            var difficultyHitObjects = new List<DifficultyHitObject>();
            var centreObjects = new List<TaikoDifficultyHitObject>();
            var rimObjects = new List<TaikoDifficultyHitObject>();
            var noteObjects = new List<TaikoDifficultyHitObject>();

            // Generate TaikoDifficultyHitObjects from the beatmap's hit objects.
            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                difficultyHitObjects.Add(new TaikoDifficultyHitObject(
                    beatmap.HitObjects[i],
                    beatmap.HitObjects[i - 1],
                    clockRate,
                    difficultyHitObjects,
                    centreObjects,
                    rimObjects,
                    noteObjects,
                    difficultyHitObjects.Count,
                    beatmap.ControlPointInfo,
                    beatmap.Difficulty.SliderMultiplier
                ));
            }

            TaikoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);
            TaikoRhythmDifficultyPreprocessor.ProcessAndAssign(noteObjects);

            return difficultyHitObjects;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods };

            var rhythm = skills.OfType<Rhythm>().Single();
            var reading = skills.OfType<Reading>().Single();
            var colour = skills.OfType<Colour>().Single();
            var stamina = skills.OfType<Stamina>().Single(s => !s.SingleColourStamina);
            var singleColourStamina = skills.OfType<Stamina>().Single(s => s.SingleColourStamina);

            double rhythmSkill = rhythm.DifficultyValue() * rhythm_skill_multiplier;
            double readingSkill = reading.DifficultyValue() * reading_skill_multiplier;
            double colourSkill = colour.DifficultyValue() * colour_skill_multiplier;
            double staminaSkill = stamina.DifficultyValue() * stamina_skill_multiplier;
            double monoStaminaSkill = singleColourStamina.DifficultyValue() * stamina_skill_multiplier;
            double monoStaminaFactor = staminaSkill == 0 ? 1 : Math.Pow(monoStaminaSkill / staminaSkill, 5);

            double staminaDifficultStrains = stamina.CountTopWeightedStrains();

            // As we don't have pattern integration in osu!taiko, we apply the other two skills relative to rhythm.
            patternMultiplier = Math.Pow(staminaSkill * colourSkill, 0.10);

            strainLengthBonus = 1 + 0.15 * DifficultyCalculationUtils.ReverseLerp(staminaDifficultStrains, 1000, 1555);

            double combinedRating = combinedDifficultyValue(rhythm, reading, colour, stamina, out double consistencyFactor);
            double starRating = rescale(combinedRating * 1.4);

            // Calculate proportional contribution of each skill to the combinedRating.
            double skillRating = starRating / (rhythmSkill + readingSkill + colourSkill + staminaSkill);

            double rhythmDifficulty = rhythmSkill * skillRating;
            double readingDifficulty = readingSkill * skillRating;
            double colourDifficulty = colourSkill * skillRating;
            double staminaDifficulty = staminaSkill * skillRating;
            double mechanicalDifficulty = colourDifficulty + staminaDifficulty; // Mechanical difficulty is the sum of colour and stamina difficulties.

            TaikoDifficultyAttributes attributes = new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                MechanicalDifficulty = mechanicalDifficulty,
                RhythmDifficulty = rhythmDifficulty,
                ReadingDifficulty = readingDifficulty,
                ColourDifficulty = colourDifficulty,
                StaminaDifficulty = staminaDifficulty,
                MonoStaminaFactor = monoStaminaFactor,
                StaminaTopStrains = staminaDifficultStrains,
                ConsistencyFactor = consistencyFactor,
                MaxCombo = beatmap.GetMaxCombo(),
            };

            return attributes;
        }

        /// <summary>
        /// Returns the combined star rating of the beatmap, calculated using peak strains from all sections of the map.
        /// </summary>
        /// <remarks>
        /// For each section, the peak strains of all separate skills are combined into a single peak strain for the section.
        /// The resulting partial rating of the beatmap is a weighted sum of the combined peaks (higher peaks are weighted more).
        /// </remarks>
        private double combinedDifficultyValue(Rhythm rhythm, Reading reading, Colour colour, Stamina stamina, out double consistencyFactor)
        {
            List<double> peaks = combinePeaks(
                rhythm.GetCurrentStrainPeaks().ToList(),
                reading.GetCurrentStrainPeaks().ToList(),
                colour.GetCurrentStrainPeaks().ToList(),
                stamina.GetCurrentStrainPeaks().ToList()
            );

            if (peaks.Count == 0)
            {
                consistencyFactor = 0;
                return 0;
            }

            double difficulty = 0;
            double weight = 1;

            foreach (double strain in peaks.OrderDescending())
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            List<double> hitObjectStrainPeaks = combinePeaks(
                rhythm.GetObjectStrains().ToList(),
                reading.GetObjectStrains().ToList(),
                colour.GetObjectStrains().ToList(),
                stamina.GetObjectStrains().ToList()
            );

            if (hitObjectStrainPeaks.Count == 0)
            {
                consistencyFactor = 0;
                return 0;
            }

            // The average of the top 5% of strain peaks from hit objects.
            double topAverageHitObjectStrain = hitObjectStrainPeaks.OrderDescending().Take(1 + hitObjectStrainPeaks.Count / 20).Average();

            // Calculates a consistency factor as the sum of difficulty from hit objects compared to if every object were as hard as the hardest.
            // The top average strain is used instead of the very hardest to prevent exceptionally hard objects lowering the factor.
            consistencyFactor = hitObjectStrainPeaks.Sum() / (topAverageHitObjectStrain * hitObjectStrainPeaks.Count);

            return difficulty;
        }

        /// <summary>
        /// Combines lists of peak strains from multiple skills into a list of single peak strains for each section.
        /// </summary>
        private List<double> combinePeaks(List<double> rhythmPeaks, List<double> readingPeaks, List<double> colourPeaks, List<double> staminaPeaks)
        {
            var combinedPeaks = new List<double>();

            for (int i = 0; i < colourPeaks.Count; i++)
            {
                double rhythmPeak = rhythmPeaks[i] * rhythm_skill_multiplier * patternMultiplier;
                double readingPeak = readingPeaks[i] * reading_skill_multiplier;
                double colourPeak = isRelax ? 0 : colourPeaks[i] * colour_skill_multiplier; // There is no colour difficulty in relax.
                double staminaPeak = staminaPeaks[i] * stamina_skill_multiplier * strainLengthBonus;
                staminaPeak /= isConvert || isRelax ? 1.5 : 1.0; // Available finger count is increased by 150%, thus we adjust accordingly.

                double peak = DifficultyCalculationUtils.Norm(2, DifficultyCalculationUtils.Norm(1.5, colourPeak, staminaPeak), rhythmPeak, readingPeak);

                // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
                // These sections will not contribute to the difficulty.
                if (peak > 0)
                    combinedPeaks.Add(peak);
            }

            return combinedPeaks;
        }

        /// <summary>
        /// Applies a final re-scaling of the star rating.
        /// </summary>
        /// <param name="sr">The raw star rating value before re-scaling.</param>
        private static double rescale(double sr)
        {
            if (sr < 0)
                return sr;

            return 10.43 * Math.Log(sr / 8 + 1);
        }
    }
}
