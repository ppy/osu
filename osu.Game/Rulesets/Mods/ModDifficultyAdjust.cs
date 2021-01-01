// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Graphics.Sprites;
using System;
using osu.Game.Configuration;
using System.Linq;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDifficultyAdjust : Mod, IApplicableToDifficulty
    {
        public override string Name => @"Difficulty Adjust";

        public override string Description => @"Override a beatmap's difficulty settings.";

        public override string Acronym => "DA";

        public override ModType Type => ModType.Conversion;

        public override IconUsage? Icon => FontAwesome.Solid.Hammer;

        public override double ScoreMultiplier => 1.0;

        public override bool RequiresConfiguration => true;

        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModHardRock) };

        protected const int FIRST_SETTING_ORDER = 1;

        protected const int LAST_SETTING_ORDER = 2;

        [SettingSource("HP Drain", "Override a beatmap's set HP.", FIRST_SETTING_ORDER)]
        public OverridableBindable<float> DrainRate { get; } = new OverridableBindable<float>(5, 0, 10, 0.1f);

        [SettingSource("Accuracy", "Override a beatmap's set OD.", LAST_SETTING_ORDER)]
        public OverridableBindable<float> OverallDifficulty { get; } = new OverridableBindable<float>(5, 0, 10, 0.1f);

        public override string SettingDescription
        {
            get
            {
                string drainRate = !DrainRate.HasCustomValue.Value ? string.Empty : $"HP {DrainRate.FinalValue.Value:N1}";
                string overallDifficulty = !OverallDifficulty.HasCustomValue.Value ? string.Empty : $"OD {OverallDifficulty.FinalValue.Value:N1}";

                return string.Join(", ", new[]
                {
                    drainRate,
                    overallDifficulty
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        private BeatmapDifficulty difficulty;

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
            if (this.difficulty == null || this.difficulty.ID != difficulty.ID)
            {
                TransferSettings(difficulty);
                this.difficulty = difficulty;
            }
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty) => ApplySettings(difficulty);

        /// <summary>
        /// Transfer initial settings from the beatmap to settings.
        /// </summary>
        /// <param name="difficulty">The beatmap's initial values.</param>
        protected virtual void TransferSettings(BeatmapDifficulty difficulty)
        {
            DrainRate.BaseValue.Value = difficulty.DrainRate;
            OverallDifficulty.BaseValue.Value = difficulty.OverallDifficulty;
        }

        /// <summary>
        /// Apply all custom settings to the provided beatmap.
        /// </summary>
        /// <param name="difficulty">The beatmap to have settings applied.</param>
        protected virtual void ApplySettings(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate = DrainRate.FinalValue.Value;
            difficulty.OverallDifficulty = OverallDifficulty.FinalValue.Value;
        }
    }
}
