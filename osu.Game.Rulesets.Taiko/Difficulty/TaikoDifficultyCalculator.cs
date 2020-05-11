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
        private const double rhythmSkillMultiplier = 0.15;
        private const double colourSkillMultiplier = 0.01;
        private const double staminaSkillMultiplier = 0.02;

        public TaikoDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        private double readingPenalty(double staminaDifficulty)
        {
            return Math.Max(0, 1 - staminaDifficulty / 14);
            // return 1;
        }

        private double norm(double p, double v1, double v2, double v3)
        {
            return Math.Pow(
                Math.Pow(v1, p) +
                Math.Pow(v2, p) +
                Math.Pow(v3, p)
                , 1 / p);
        }

        private double rescale(double sr)
        {
            if (sr <= 1) return sr;
            sr -= 1;
            sr = 1.5 * Math.Pow(sr, 0.76);
            sr += 1;
            return sr;
        }

        private double combinedDifficulty(Skill colour, Skill rhythm, Skill stamina1, Skill stamina2)
        {
            double staminaRating = (stamina1.DifficultyValue() + stamina2.DifficultyValue()) * staminaSkillMultiplier;
            double readingPenalty = this.readingPenalty(staminaRating);

            double difficulty = 0;
            double weight = 1;
            List<double> peaks = new List<double>();

            for (int i = 0; i < colour.StrainPeaks.Count; i++)
            {
                double colourPeak = colour.StrainPeaks[i] * colourSkillMultiplier * readingPenalty;
                double rhythmPeak = rhythm.StrainPeaks[i] * rhythmSkillMultiplier;
                double staminaPeak = (stamina1.StrainPeaks[i] + stamina2.StrainPeaks[i]) * staminaSkillMultiplier;
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

            double staminaRating = (skills[2].DifficultyValue() + skills[3].DifficultyValue()) * staminaSkillMultiplier;
            double readingPenalty = this.readingPenalty(staminaRating);

            double colourRating = skills[0].DifficultyValue() * colourSkillMultiplier * readingPenalty;
            double rhythmRating = skills[1].DifficultyValue() * rhythmSkillMultiplier;
            double combinedRating = combinedDifficulty(skills[0], skills[1], skills[2], skills[3]);

            // Console.WriteLine("colour\t" + colourRating);
            // Console.WriteLine("rhythm\t" + rhythmRating);
            // Console.WriteLine("stamina\t" + staminaRating);
            double separatedRating = norm(1.5, colourRating, rhythmRating, staminaRating);
            // Console.WriteLine("combinedRating\t" + combinedRating);
            // Console.WriteLine("separatedRating\t" + separatedRating);
            double starRating = 1.4 * separatedRating + 0.5 * combinedRating;
            starRating = rescale(starRating);

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            return new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
                GreatHitWindow = (int)(hitWindows.WindowFor(HitResult.Great)) / clockRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit),
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            List<TaikoDifficultyHitObject> taikoDifficultyHitObjects = new List<TaikoDifficultyHitObject>();
            var rhythm = new TaikoDifficultyHitObjectRhythm();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                taikoDifficultyHitObjects.Add(new TaikoDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate, rhythm));
            }

            new StaminaCheeseDetector().FindCheese(taikoDifficultyHitObjects);
            for (int i = 0; i < taikoDifficultyHitObjects.Count; i++)
                yield return taikoDifficultyHitObjects[i];
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

        /*
        protected override DifficultyAttributes VirtualCalculate(IBeatmap beatmap, Mod[] mods, double clockRate)
            => taikoCalculate(beatmap, mods, clockRate);
        */
    }
}
