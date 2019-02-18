// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private const double star_scaling_factor = 0.04125;

        public TaikoDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override void PopulateAttributes(DifficultyAttributes attributes, IBeatmap beatmap, Skill[] skills, double timeRate)
        {
            var taikoAttributes = (TaikoDifficultyAttributes)attributes;

            taikoAttributes.StarRating = skills.Single().DifficultyValue() * star_scaling_factor;

            // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            taikoAttributes.GreatHitWindow = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / timeRate;
            taikoAttributes.MaxCombo = beatmap.HitObjects.Count(h => h is Hit);
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double timeRate)
        {
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
                yield return new TaikoDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], timeRate);
        }

        protected override Skill[] CreateSkills() => new Skill[] { new Strain() };

        protected override DifficultyAttributes CreateDifficultyAttributes(Mod[] mods) => new TaikoDifficultyAttributes(mods);

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };
    }
}
