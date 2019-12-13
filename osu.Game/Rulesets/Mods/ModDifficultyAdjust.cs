// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using System;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public class ModDifficultyAdjust : Mod, IApplicableToDifficulty
    {
        public override string Name => @"Difficulty Adjust";

        public override string Description => @"Override a beatmap's difficulty settings";

        public override string Acronym => "DA";

        public override ModType Type => ModType.Conversion;

        public override IconUsage Icon => FontAwesome.Solid.Hammer;

        public override double ScoreMultiplier => 1.0;

        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModHardRock) };

        [SettingSource("Drain Rate", "Override the beatmap's set HP")]
        public BindableNumber<float> DrainRate { get; } = new BindableFloat
        {
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
            Precision = 0.1F,
        };

        [SettingSource("Circle Size", "Override the beatmap's set CS")]
        public BindableNumber<float> CircleSize { get; } = new BindableFloat
        {
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
            Precision = 0.1F,
        };

        [SettingSource("Approach Rate", "Override the beatmap's set AR")]
        public BindableNumber<float> ApproachRate { get; } = new BindableFloat
        {
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
            Precision = 0.1F,
        };

        [SettingSource("Overall Difficulty", "Override the beatmap's set OD")]
        public BindableNumber<float> OverallDifficulty { get; } = new BindableFloat
        {
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
            Precision = 0.1F,
        };

        private BeatmapDifficulty difficulty;

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (this.difficulty == null || this.difficulty.ID != difficulty.ID)
            {
                this.difficulty = difficulty;
                DrainRate.Value = DrainRate.Default = difficulty.DrainRate;
                CircleSize.Value = CircleSize.Default = difficulty.CircleSize;
                ApproachRate.Value = ApproachRate.Default = difficulty.ApproachRate;
                OverallDifficulty.Value = OverallDifficulty.Default = difficulty.OverallDifficulty;
            }
            else
            {
                difficulty.DrainRate = DrainRate.Value;
                difficulty.CircleSize = CircleSize.Value;
                difficulty.ApproachRate = ApproachRate.Value;
                difficulty.OverallDifficulty = OverallDifficulty.Value;
            }
        }
    }
}
