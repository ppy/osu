// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Scroll Speed", "Adjust a beatmap's set scroll speed", LAST_SETTING_ORDER + 1, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable ScrollSpeed { get; } = new DifficultyBindable
        {
            Precision = 0.05f,
            MinValue = 0.25f,
            MaxValue = 4,
            ReadCurrentFromDifficulty = _ => 1,
        };

        public override string ExtendedIconInformation
        {
            get
            {
                if (!IsExactlyOneSettingChanged(ScrollSpeed, OverallDifficulty, DrainRate))
                    return string.Empty;

                if (!ScrollSpeed.IsDefault) return format("SC", ScrollSpeed, 2);
                if (!OverallDifficulty.IsDefault) return format("OD", OverallDifficulty, 1);
                if (!DrainRate.IsDefault) return format("HP", DrainRate, 1);

                return string.Empty;

                string format(string acronym, DifficultyBindable bindable, int digits)
                    => $"{acronym}{bindable.Value!.Value.ToStandardFormattedString(digits)}";
            }
        }

        public override IEnumerable<(LocalisableString setting, LocalisableString value)> SettingDescription
        {
            get
            {
                foreach (var setting in base.SettingDescription)
                    yield return setting;

                if (!ScrollSpeed.IsDefault)
                    yield return ("Scroll speed", $"x{ScrollSpeed.Value:N2}");
            }
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            if (ScrollSpeed.Value != null) difficulty.SliderMultiplier *= ScrollSpeed.Value.Value;
        }
    }
}
