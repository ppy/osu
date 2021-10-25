// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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

        public override string SettingDescription
        {
            get
            {
                string scrollSpeed = ScrollSpeed.IsDefault ? string.Empty : $"Scroll x{ScrollSpeed.Value:N1}";

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    scrollSpeed
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            if (ScrollSpeed.Value != null) difficulty.SliderMultiplier *= ScrollSpeed.Value.Value;
        }
    }
}
