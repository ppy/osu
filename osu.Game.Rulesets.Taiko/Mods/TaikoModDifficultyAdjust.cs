// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Slider Velocity", "Adjust a beatmap's set SV", LAST_SETTING_ORDER + 1)]
        public BindableNumber<float> SliderVelocity { get; } = new BindableFloat
        {
            Precision = 0.05f,
            MinValue = 0.25f,
            MaxValue = 4,
            Default = 1,
            Value = 1,
        };

        public override string SettingDescription
        {
            get
            {
                string sliderVelocity = SliderVelocity.IsDefault ? string.Empty : $"SV {SliderVelocity.Value:N1}";

                return string.Join(", ", new[]
                {
                    base.SettingDescription,
                    sliderVelocity
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            ApplySetting(SliderVelocity, sv => difficulty.SliderMultiplier *= sv);
        }
    }
}
