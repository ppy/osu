// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

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

        [SettingSource("HP Drain", "Override a beatmap's set HP.", FIRST_SETTING_ORDER, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public Bindable<float?> DrainRate { get; } = new Bindable<float?>
        {
            /*
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            */
        };

        [SettingSource("Accuracy", "Override a beatmap's set OD.", LAST_SETTING_ORDER, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public Bindable<float?> OverallDifficulty { get; } = new Bindable<float?>
        {
            /*
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            */
        };

        [SettingSource("Extended Limits", "Adjust difficulty beyond sane limits.")]
        public BindableBool ExtendedLimits { get; } = new BindableBool();

        protected ModDifficultyAdjust()
        {
            ExtendedLimits.BindValueChanged(extend => ApplyLimits(extend.NewValue));
        }

        /// <summary>
        /// Changes the difficulty adjustment limits. Occurs when the value of <see cref="ExtendedLimits"/> is changed.
        /// </summary>
        /// <param name="extended">Whether limits should extend beyond sane ranges.</param>
        protected virtual void ApplyLimits(bool extended)
        {
            // TODO: reimplement
            // DrainRate.MaxValue = extended ? 11 : 10;
            // OverallDifficulty.MaxValue = extended ? 11 : 10;
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

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
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

        /// <summary>
        /// A <see cref="BindableDouble"/> that extends its min/max values to support any assigned value.
        /// </summary>
        protected class BindableDoubleWithLimitExtension : BindableDouble
        {
            public override double Value
            {
                get => base.Value;
                set
                {
                    if (value < MinValue)
                        MinValue = value;
                    if (value > MaxValue)
                        MaxValue = value;
                    base.Value = value;
                }
            }
        }

        /// <summary>
        /// A <see cref="BindableFloat"/> that extends its min/max values to support any assigned value.
        /// </summary>
        protected class BindableFloatWithLimitExtension : BindableFloat
        {
            public override float Value
            {
                get => base.Value;
                set
                {
                    if (value < MinValue)
                        MinValue = value;
                    if (value > MaxValue)
                        MaxValue = value;
                    base.Value = value;
                }
            }
        }

        /// <summary>
        /// A <see cref="BindableInt"/> that extends its min/max values to support any assigned value.
        /// </summary>
        protected class BindableIntWithLimitExtension : BindableInt
        {
            public override int Value
            {
                get => base.Value;
                set
                {
                    if (value < MinValue)
                        MinValue = value;
                    if (value > MaxValue)
                        MaxValue = value;
                    base.Value = value;
                }
            }
        }
    }
}
