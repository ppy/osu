// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        private const double rhythm_skill_multiplier = 0.014;
        private const double colour_skill_multiplier = 0.01;
        private const double stamina_skill_multiplier = 0.02;

        public TaikoDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => new Skill[]
        {
            new Colour(mods),
            new Rhythm(mods),
            new Stamina(mods, true),
            new Stamina(mods, false),
        };

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<TaikoDifficultyHitObject> taikoDifficultyHitObjects = new List<TaikoDifficultyHitObject>();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                taikoDifficultyHitObjects.Add(
                    new TaikoDifficultyHitObject(
                        beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate, i
                    )
                );
            }

            new StaminaCheeseDetector(taikoDifficultyHitObjects).FindCheese();
            return taikoDifficultyHitObjects;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods, Skills = skills };

            var colour = (Colour)skills[0];
            var rhythm = (Rhythm)skills[1];
            var staminaRight = (Stamina)skills[2];
            var staminaLeft = (Stamina)skills[3];

            double colourRating = colour.DifficultyValue() * colour_skill_multiplier;
            double rhythmRating = rhythm.DifficultyValue() * rhythm_skill_multiplier;
            double staminaRating = (staminaRight.DifficultyValue() + staminaLeft.DifficultyValue()) * stamina_skill_multiplier;

            double staminaPenalty = simpleColourPenalty(staminaRating, colourRating);
            staminaRating *= staminaPenalty;

            double combinedRating = locallyCombinedDifficulty(colour, rhythm, staminaRight, staminaLeft, staminaPenalty);
            double separatedRating = norm(1.5, colourRating, rhythmRating, staminaRating);
            double starRating = 1.4 * separatedRating + 0.5 * combinedRating;
            starRating = rescale(starRating);

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            return new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                StaminaStrain = staminaRating,
                RhythmStrain = rhythmRating,
                ColourStrain = colourRating,
                GreatHitWindow = hitWindows.WindowFor(HitResult.Great) / clockRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit),
                Skills = skills
            };
        }

        /// <summary>
        /// Calculates the penalty for the stamina skill for maps with low colour difficulty.
        /// </summary>
        /// <remarks>
        /// Some maps (especially converts) can be easy to read despite a high note density.
        /// This penalty aims to reduce the star rating of such maps by factoring in colour difficulty to the stamina skill.
        /// </remarks>
        private double simpleColourPenalty(double staminaDifficulty, double colorDifficulty)
        {
            if (colorDifficulty <= 0) return 0.79 - 0.25;

            return 0.79 - Math.Atan(staminaDifficulty / colorDifficulty - 12) / Math.PI / 2;
        }

        /// <summary>
        /// Returns the <i>p</i>-norm of an <i>n</i>-dimensional vector.
        /// </summary>
        /// <param name="p">The value of <i>p</i> to calculate the norm for.</param>
        /// <param name="values">The coefficients of the vector.</param>
        private double norm(double p, params double[] values) => Math.Pow(values.Sum(x => Math.Pow(x, p)), 1 / p);

        /// <summary>
        /// Returns the partial star rating of the beatmap, calculated using peak strains from all sections of the map.
        /// </summary>
        /// <remarks>
        /// For each section, the peak strains of all separate skills are combined into a single peak strain for the section.
        /// The resulting partial rating of the beatmap is a weighted sum of the combined peaks (higher peaks are weighted more).
        /// </remarks>
        private double locallyCombinedDifficulty(Colour colour, Rhythm rhythm, Stamina staminaRight, Stamina staminaLeft, double staminaPenalty)
        {
            List<double> peaks = new List<double>();

            var colourPeaks = colour.GetCurrentStrainPeaks().ToList();
            var rhythmPeaks = rhythm.GetCurrentStrainPeaks().ToList();
            var staminaRightPeaks = staminaRight.GetCurrentStrainPeaks().ToList();
            var staminaLeftPeaks = staminaLeft.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < colourPeaks.Count; i++)
            {
                double colourPeak = colourPeaks[i] * colour_skill_multiplier;
                double rhythmPeak = rhythmPeaks[i] * rhythm_skill_multiplier;
                double staminaPeak = (staminaRightPeaks[i] + staminaLeftPeaks[i]) * stamina_skill_multiplier * staminaPenalty;
                peaks.Add(norm(2, colourPeak, rhythmPeak, staminaPeak));
            }

            double difficulty = 0;
            double weight = 1;

            foreach (double strain in peaks.OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }

        /// <summary>
        /// Applies a final re-scaling of the star rating to bring maps with recorded full combos below 9.5 stars.
        /// </summary>
        /// <param name="sr">The raw star rating value before re-scaling.</param>
        private double rescale(double sr)
        {
            if (sr < 0) return sr;

            return 10.43 * Math.Log(sr / 8 + 1);
        }
    }
}
