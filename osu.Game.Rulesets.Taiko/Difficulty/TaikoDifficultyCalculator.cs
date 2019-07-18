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

			var readingRating = skills[0].DifficultyValue() * star_scaling_factor;
			var speedRating = skills[1].DifficultyValue() * star_scaling_factor;
            return new TaikoDifficultyAttributes
            {
				// Scale SR to stay somewhat consistent with how it used to be
                StarRating = Math.Pow(Math.Pow(readingRating, 1.2) / 3.6, 0.5) * 3.6 + speedRating,
                Mods = mods,
				ReadingStrain = readingRating,
				SpeedStrain = speedRating,
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
                GreatHitWindow = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / clockRate,
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
