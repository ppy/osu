// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
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
        public BindableNumber<float> DrainRate { get; } = new BindableFloatWithLimitExtension
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Accuracy", "Override a beatmap's set OD.", LAST_SETTING_ORDER)]
        public BindableNumber<float> OverallDifficulty { get; } = new BindableFloatWithLimitExtension
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            Default = 5,
            Value = 5,
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
            DrainRate.MaxValue = extended ? 11 : 10;
            OverallDifficulty.MaxValue = extended ? 11 : 10;
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
            TransferSetting(DrainRate, difficulty.DrainRate);
            TransferSetting(OverallDifficulty, difficulty.OverallDifficulty);
        }

        private readonly Dictionary<IBindable, bool> userChangedSettings = new Dictionary<IBindable, bool>();

        /// <summary>
        /// Transfer a setting from <see cref="BeatmapDifficulty"/> to a configuration bindable.
        /// Only performs the transfer if the user is not currently overriding.
        /// </summary>
        protected void TransferSetting<T>(BindableNumber<T> bindable, T beatmapDefault)
            where T : struct, IComparable<T>, IConvertible, IEquatable<T>
        {
            bindable.UnbindEvents();

            userChangedSettings.TryAdd(bindable, false);

            bindable.Default = beatmapDefault;

            // users generally choose a difficulty setting and want it to stick across multiple beatmap changes.
            // we only want to value transfer if the user hasn't changed the value previously.
            if (!userChangedSettings[bindable])
                bindable.Value = beatmapDefault;

            bindable.ValueChanged += _ => userChangedSettings[bindable] = !bindable.IsDefault;
        }

        internal override void CopyAdjustedSetting(IBindable target, object source)
        {
            // if the value is non-bindable, it's presumably coming from an external source (like the API) - therefore presume it is not default.
            // if the value is bindable, defer to the source's IsDefault to be able to tell.
            userChangedSettings[target] = !(source is IBindable bindableSource) || !bindableSource.IsDefault;
            base.CopyAdjustedSetting(target, source);
        }

        /// <summary>
        /// Applies a setting from a configuration bindable using <paramref name="applyFunc"/>, if it has been changed by the user.
        /// </summary>
        protected void ApplySetting<T>(BindableNumber<T> setting, Action<T> applyFunc)
            where T : struct, IComparable<T>, IConvertible, IEquatable<T>
        {
            if (userChangedSettings.TryGetValue(setting, out bool userChangedSetting) && userChangedSetting)
                applyFunc.Invoke(setting.Value);
        }

        /// <summary>
        /// Apply all custom settings to the provided beatmap.
        /// </summary>
        /// <param name="difficulty">The beatmap to have settings applied.</param>
        protected virtual void ApplySettings(BeatmapDifficulty difficulty)
        {
            ApplySetting(DrainRate, dr => difficulty.DrainRate = dr);
            ApplySetting(OverallDifficulty, od => difficulty.OverallDifficulty = od);
        }

        public override void ResetSettingsToDefaults()
        {
            base.ResetSettingsToDefaults();

            if (difficulty != null)
            {
                // base implementation potentially overwrite modified defaults that came from a beatmap selection.
                TransferSettings(difficulty);
            }
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
