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
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.028;

        public TaikoDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods, Skills = skills };

            // Reading needs to be calculated first to detect easy patterns spam for speed.
            // Scale it to stay somewhat consistent with how it used to be.
            var readingRating = Math.Pow(skills[0].DifficultyValue() * star_scaling_factor + 1, 0.5) - 1;
            var speedRating = skills[1].DifficultyValue() * star_scaling_factor;

            var starRating = readingRating + speedRating;
            var lengthBonus = 0.0;

            for (var i = 0; i < skills[0].StrainPeaks.Count; i++)
                lengthBonus += (skills[0].StrainPeaks[i] + skills[1].StrainPeaks[i]) * Math.Pow(1.0003, i);

            // Average peak
            lengthBonus /= skills[0].StrainPeaks.Count;
            // Scale with object count
            lengthBonus *= beatmap.HitObjects.Count(h => h is Hit);
            // Reduce bonus if the object count is low for this SR
            lengthBonus *= Math.Pow((beatmap.HitObjects.Count(h => h is Hit) / (Math.Pow(1.18, starRating + 1) * 777.0 - 916.86)), 0.8);
            lengthBonus = Math.Pow(1 + Math.Min(4.0, lengthBonus / starRating / 16000.0), 0.5) - 0.5;

            return new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                ReadingStrain = readingRating,
                SpeedStrain = speedRating,
                LengthBonus = lengthBonus,
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
                GreatHitWindow = ((int)(beatmap.HitObjects.First().HitWindows.Great / 2) - 0.5) / clockRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit),
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
                yield return new TaikoDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], clockRate);
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[] { new Reading(), new Speed() };

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };
    }
}
