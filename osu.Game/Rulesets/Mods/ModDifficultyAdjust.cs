// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Bindables;
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
        public DifficultyTrackingBindable<float> DrainRate { get; } = new DifficultyTrackingBindable<float>
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
        };

        [SettingSource("Accuracy", "Override a beatmap's set OD.", LAST_SETTING_ORDER)]
        public DifficultyTrackingBindable<float> OverallDifficulty { get; } = new DifficultyTrackingBindable<float>
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
        };

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
            DrainRate.ChangeBase(difficulty.DrainRate);
            OverallDifficulty.ChangeBase(difficulty.OverallDifficulty);
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

        public class DifficultyTrackingBindable<T> : BindableNumber<T>
            where T : struct, IComparable<T>, IConvertible, IEquatable<T>
        {
            private bool hasBaseValue;

            /// <summary>
            /// Whether a value different from the base difficulty has been set.
            /// </summary>
            public bool Overriden { get; private set; }

            public override T Value
            {
                get => base.Value;
                set
                {
                    base.Value = value;

                    // if there's no base value, consider any touches to this property a setting override.
                    if (!hasBaseValue)
                        Overriden = true;
                }
            }

            public void ChangeBase(T newBase)
            {
                UnbindEvents();

                Default = newBase;

                // if this wasn't provided a base before, and current value matches new base,
                // then the setting can be considered not overriden.
                if (!hasBaseValue && IsDefault)
                    Overriden = false;

                hasBaseValue = true;

                // users generally choose a difficulty setting and want it to stick across multiple beatmap changes.
                // we only want to value transfer if the user hasn't changed the value previously.
                if (!Overriden)
                    Value = newBase;

                ValueChanged += _ => Overriden = !IsDefault;
            }
        }
    }
}
