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
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Reading;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.084375;
        private const double rhythm_skill_multiplier = 1.24 * difficulty_multiplier;
        private const double reading_skill_multiplier = 0.100 * difficulty_multiplier;
        private const double colour_skill_multiplier = 0.375 * difficulty_multiplier;
        private const double stamina_skill_multiplier = 0.375 * difficulty_multiplier;

        public override int Version => 20241007;

        public TaikoDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            HitWindows hitWindows = new HitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            return new Skill[]
            {
                new Rhythm(mods, hitWindows.WindowFor(HitResult.Great) / clockRate),
                new Reading(mods),
                new Colour(mods),
                new Stamina(mods, false),
                new Stamina(mods, true)
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
            var hitWindows = new HitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            var difficultyHitObjects = new List<DifficultyHitObject>();
            var centreObjects = new List<TaikoDifficultyHitObject>();
            var rimObjects = new List<TaikoDifficultyHitObject>();
            var noteObjects = new List<TaikoDifficultyHitObject>();
            EffectiveBPMPreprocessor bpmLoader = new EffectiveBPMPreprocessor(beatmap, noteObjects);

            // Generate TaikoDifficultyHitObjects from the beatmap's hit objects.
            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                difficultyHitObjects.Add(new TaikoDifficultyHitObject(
                    beatmap.HitObjects[i],
                    beatmap.HitObjects[i - 1],
                    beatmap.HitObjects[i - 2],
                    clockRate,
                    difficultyHitObjects,
                    centreObjects,
                    rimObjects,
                    noteObjects,
                    difficultyHitObjects.Count
                ));
            }

            var groupedHitObjects = SameRhythmHitObjects.GroupHitObjects(noteObjects);

            TaikoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);
            SamePatterns.GroupPatterns(groupedHitObjects);
            bpmLoader.ProcessEffectiveBPM(beatmap.ControlPointInfo, clockRate);

            return difficultyHitObjects;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods };

            bool isRelax = mods.Any(h => h is TaikoModRelax);

            Rhythm rhythm = (Rhythm)skills.First(x => x is Rhythm);
            Reading reading = (Reading)skills.First(x => x is Reading);
            Colour colour = (Colour)skills.First(x => x is Colour);
            Stamina stamina = (Stamina)skills.First(x => x is Stamina);
            Stamina singleColourStamina = (Stamina)skills.Last(x => x is Stamina);

            double rhythmRating = rhythm.DifficultyValue() * rhythm_skill_multiplier;
            double readingRating = reading.DifficultyValue() * reading_skill_multiplier;
            double colourRating = colour.DifficultyValue() * colour_skill_multiplier;
            double staminaRating = stamina.DifficultyValue() * stamina_skill_multiplier;
            double monoStaminaRating = singleColourStamina.DifficultyValue() * stamina_skill_multiplier;
            double monoStaminaFactor = staminaRating == 0 ? 1 : Math.Pow(monoStaminaRating / staminaRating, 5);

            double colourDifficultStrains = colour.CountTopWeightedStrains();
            double readingDifficultStrains = reading.CountTopWeightedStrains();
            double staminaDifficultStrains = stamina.CountTopWeightedStrains();

            double combinedRating = combinedDifficultyValue(rhythm, reading, colour, stamina, isRelax);
            double starRating = rescale(combinedRating * 1.4);

            // Converts are penalised outside the scope of difficulty calculation, as our assumptions surrounding standard play-styles becomes out-of-scope.
            if (beatmap.BeatmapInfo.Ruleset.OnlineID == 0)
            {
                starRating *= 0.825;

                // For maps with relax, multiple inputs are more likely to be abused.
                if (isRelax)
                    starRating *= 0.60;
            }

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            TaikoDifficultyAttributes attributes = new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                RhythmDifficulty = rhythmRating,
                ReadingDifficulty = readingRating,
                ColourDifficulty = colourRating,
                StaminaDifficulty = staminaRating,
                MonoStaminaFactor = monoStaminaFactor,
                ReadingTopStrains = readingDifficultStrains,
                ColourTopStrains = colourDifficultStrains,
                StaminaTopStrains = staminaDifficultStrains,
                GreatHitWindow = hitWindows.WindowFor(HitResult.Great) / clockRate,
                OkHitWindow = hitWindows.WindowFor(HitResult.Ok) / clockRate,
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
        private double combinedDifficultyValue(Rhythm rhythm, Reading reading, Colour colour, Stamina stamina, bool isRelax)
        {
            List<double> peaks = new List<double>();

            var rhythmPeaks = rhythm.GetCurrentStrainPeaks().ToList();
            var readingPeaks = reading.GetCurrentStrainPeaks().ToList();
            var colourPeaks = colour.GetCurrentStrainPeaks().ToList();
            var staminaPeaks = stamina.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < colourPeaks.Count; i++)
            {
                double rhythmPeak = rhythmPeaks[i] * rhythm_skill_multiplier;
                double readingPeak = readingPeaks[i] * reading_skill_multiplier;
                double colourPeak = colourPeaks[i] * colour_skill_multiplier;
                double staminaPeak = staminaPeaks[i] * stamina_skill_multiplier;

                if (isRelax)
                {
                    colourPeak = 0; // There is no colour difficulty in relax.
                    staminaPeak /= 1.5; // Stamina difficulty is decreased with an increased available finger count.
                }

                double peak = DifficultyCalculationUtils.Norm(2, DifficultyCalculationUtils.Norm(1.5, colourPeak, staminaPeak), rhythmPeak, readingPeak);

                // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
                // These sections will not contribute to the difficulty.
                if (peak > 0)
                    peaks.Add(peak);
            }

            double difficulty = 0;
            double weight = 1;

            foreach (double strain in peaks.OrderDescending())
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }

        /// <summary>
        /// Applies a final re-scaling of the star rating.
        /// </summary>
        /// <param name="sr">The raw star rating value before re-scaling.</param>
        private double rescale(double sr)
        {
            if (sr < 0) return sr;

            return 10.43 * Math.Log(sr / 8 + 1);
        }
    }
}
