// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using System;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDifficultyAdjust : Mod, IApplicableToDifficulty
    {
        public override string Name => @"Difficulty Adjust";

        public override string Description => @"Override a beatmap's difficulty settings.";

        public override string Acronym => "DA";

        public override ModType Type => ModType.Conversion;

        public override IconUsage Icon => FontAwesome.Solid.Hammer;

        public override double ScoreMultiplier => 1.0;

        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModHardRock) };

        [SettingSource("Drain Rate", "Override a beatmap's set HP.")]
        public BindableNumber<float> DrainRate { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Overall Difficulty", "Override a beatmap's set OD.")]
        public BindableNumber<float> OverallDifficulty { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        private BeatmapDifficulty difficulty;

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
            if (this.difficulty == null || this.difficulty.ID != difficulty.ID)
            {
                this.difficulty = difficulty;
                TransferSettings(difficulty);
            }
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty) => ApplySettings(difficulty);

        /// <summary>
        /// Transfer initial settings from the beatmap to settings.
        /// </summary>
        /// <param name="difficulty">The beatmap's initial values.</param>
        protected virtual void TransferSettings(BeatmapDifficulty difficulty)
        {
            DrainRate.Value = DrainRate.Default = difficulty.DrainRate;
            OverallDifficulty.Value = OverallDifficulty.Default = difficulty.OverallDifficulty;
        }

        /// <summary>
        /// Apply all custom settings to the provided beatmap.
        /// </summary>
        /// <param name="difficulty">The beatmap to have settings applied.</param>
        protected virtual void ApplySettings(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate = DrainRate.Value;
            difficulty.OverallDifficulty = OverallDifficulty.Value;
        }
    }
}
