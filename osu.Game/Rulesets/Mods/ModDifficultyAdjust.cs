// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
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

        [SettingSource("Drain Rate", "Override a beatmap's set HP.", SettingSourceAttribute.ORDERED_RELATIVE, 1)]
        public BindableNumber<float> DrainRate { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Overall Difficulty", "Override a beatmap's set OD.", SettingSourceAttribute.ORDERED_RELATIVE, 1)]
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
        /// Only performs the transfer if the user it not currently overriding..
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
