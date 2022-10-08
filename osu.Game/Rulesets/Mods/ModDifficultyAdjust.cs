// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDifficultyAdjust : Mod, IApplicableToDifficulty
    {
        public override string Name => @"难度调整";

        public override LocalisableString Description =>  @"调整谱面的难度设定";

        public override string Acronym => "DA";

        public override ModType Type => ModType.Conversion;

        public override IconUsage? Icon => FontAwesome.Solid.Hammer;

        public override double ScoreMultiplier => 0.5;

        public override bool RequiresConfiguration => true;

        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModHardRock) };

        protected const int FIRST_SETTING_ORDER = 1;

        protected const int LAST_SETTING_ORDER = 2;

        [SettingSource("掉血速度", "覆盖谱面的HP设置", FIRST_SETTING_ORDER, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable DrainRate { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            ExtendedMaxValue = 11,
            ReadCurrentFromDifficulty = diff => diff.DrainRate,
        };

        [SettingSource("整体难度", "覆盖谱面的OD设置", LAST_SETTING_ORDER, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable OverallDifficulty { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            ExtendedMaxValue = 11,
            ReadCurrentFromDifficulty = diff => diff.OverallDifficulty,
        };

        [SettingSource("扩展限制", "允许你将一些数值调整到变态难度")]
        public BindableBool ExtendedLimits { get; } = new BindableBool();

        protected ModDifficultyAdjust()
        {
            foreach (var (_, property) in this.GetOrderedSettingsSourceProperties())
            {
                if (property.GetValue(this) is DifficultyBindable diffAdjustBindable)
                    diffAdjustBindable.ExtendedLimits.BindTo(ExtendedLimits);
            }
        }

        public override string SettingDescription
        {
            get
            {
                string drainRate = DrainRate.IsDefault ? string.Empty : $"HP {DrainRate.Value:N1}";
                string overallDifficulty = OverallDifficulty.IsDefault ? string.Empty : $"OD {OverallDifficulty.Value:N1}";

                return string.Join(", ", new[]
                {
                    drainRate,
                    overallDifficulty
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public void ReadFromDifficulty(IBeatmapDifficultyInfo difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty) => ApplySettings(difficulty);

        /// <summary>
        /// Apply all custom settings to the provided beatmap.
        /// </summary>
        /// <param name="difficulty">The beatmap to have settings applied.</param>
        protected virtual void ApplySettings(BeatmapDifficulty difficulty)
        {
            if (DrainRate.Value != null) difficulty.DrainRate = DrainRate.Value.Value;
            if (OverallDifficulty.Value != null) difficulty.OverallDifficulty = OverallDifficulty.Value.Value;
        }
    }
}
