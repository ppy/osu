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

        private readonly TaikoDifficultyHitObjectRhythm[] commonRhythms =
        {
            new TaikoDifficultyHitObjectRhythm(1, 1, 0.0),
            new TaikoDifficultyHitObjectRhythm(2, 1, 0.3),
            new TaikoDifficultyHitObjectRhythm(1, 2, 0.5),
            new TaikoDifficultyHitObjectRhythm(3, 1, 0.3),
            new TaikoDifficultyHitObjectRhythm(1, 3, 0.35),
            new TaikoDifficultyHitObjectRhythm(3, 2, 0.6),
            new TaikoDifficultyHitObjectRhythm(2, 3, 0.4),
            new TaikoDifficultyHitObjectRhythm(5, 4, 0.5),
            new TaikoDifficultyHitObjectRhythm(4, 5, 0.7)
        };

        public TaikoDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        private double simpleColourPenalty(double staminaDifficulty, double colorDifficulty)
        {
            if (colorDifficulty <= 0) return 0.79 - 0.25;

            return 0.79 - Math.Atan(staminaDifficulty / colorDifficulty - 12) / Math.PI / 2;
        }

        private double norm(double p, params double[] values)
        {
            return Math.Pow(values.Sum(x => Math.Pow(x, p)), 1 / p);
        }

        private double rescale(double sr)
        {
            if (sr < 0) return sr;

            return 10.43 * Math.Log(sr / 8 + 1);
        }

        private double locallyCombinedDifficulty(double staminaPenalty, Skill colour, Skill rhythm, Skill stamina1, Skill stamina2)
        {
            double difficulty = 0;
            double weight = 1;
            List<double> peaks = new List<double>();

            for (int i = 0; i < colour.StrainPeaks.Count; i++)
            {
                double colourPeak = colour.StrainPeaks[i] * colour_skill_multiplier;
                double rhythmPeak = rhythm.StrainPeaks[i] * rhythm_skill_multiplier;
                double staminaPeak = (stamina1.StrainPeaks[i] + stamina2.StrainPeaks[i]) * stamina_skill_multiplier * staminaPenalty;
                peaks.Add(norm(2, colourPeak, rhythmPeak, staminaPeak));
            }

            foreach (double strain in peaks.OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return difficulty;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods, Skills = skills };

            double colourRating = skills[0].DifficultyValue() * colour_skill_multiplier;
            double rhythmRating = skills[1].DifficultyValue() * rhythm_skill_multiplier;
            double staminaRating = (skills[2].DifficultyValue() + skills[3].DifficultyValue()) * stamina_skill_multiplier;

            double staminaPenalty = simpleColourPenalty(staminaRating, colourRating);
            staminaRating *= staminaPenalty;

            double combinedRating = locallyCombinedDifficulty(staminaPenalty, skills[0], skills[1], skills[2], skills[3]);
            double separatedRating = norm(1.5, colourRating, rhythmRating, staminaRating);
            double starRating = 1.4 * separatedRating + 0.5 * combinedRating;
            starRating = rescale(starRating);

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            return new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                StaminaStrain = staminaRating,
                RhythmStrain = rhythmRating,
                ColourStrain = colourRating,
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
                GreatHitWindow = (int)hitWindows.WindowFor(HitResult.Great) / clockRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit),
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<TaikoDifficultyHitObject> taikoDifficultyHitObjects = new List<TaikoDifficultyHitObject>();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                // Check for negative durations
                if (beatmap.HitObjects[i].StartTime > beatmap.HitObjects[i - 1].StartTime && beatmap.HitObjects[i - 1].StartTime > beatmap.HitObjects[i - 2].StartTime)
                {
                    taikoDifficultyHitObjects.Add(
                        new TaikoDifficultyHitObject(
                            beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate, i, commonRhythms
                        )
                    );
                }
            }

            new StaminaCheeseDetector().FindCheese(taikoDifficultyHitObjects);
            foreach (var hitobject in taikoDifficultyHitObjects)
                yield return hitobject;
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new Colour(),
            new Rhythm(),
            new Stamina(true),
            new Stamina(false),
        };

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };
    }
}
