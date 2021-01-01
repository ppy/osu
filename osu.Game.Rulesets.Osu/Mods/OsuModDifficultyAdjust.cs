// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Circle Size", "Override a beatmap's set CS.", FIRST_SETTING_ORDER - 1)]
        public OverridableBindable<float> CircleSize { get; } = new OverridableBindable<float>(5, 0, 10, 0.1f);

        [SettingSource("Approach Rate", "Override a beatmap's set AR.", LAST_SETTING_ORDER + 1)]
        public OverridableBindable<float> ApproachRate { get; } = new OverridableBindable<float>(5, 0, 10, 0.1f);

        public override string SettingDescription
        {
            get
            {
                string circleSize = !CircleSize.HasCustomValue.Value ? string.Empty : $"CS {CircleSize.FinalValue.Value:N1}";
                string approachRate = !ApproachRate.HasCustomValue.Value ? string.Empty : $"AR {ApproachRate.FinalValue.Value:N1}";

                return string.Join(", ", new[]
                {
                    circleSize,
                    base.SettingDescription,
                    approachRate
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        protected override void TransferSettings(BeatmapDifficulty difficulty)
        {
            base.TransferSettings(difficulty);

            CircleSize.BaseValue.Value = difficulty.CircleSize;
            ApproachRate.BaseValue.Value = difficulty.ApproachRate;
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            difficulty.CircleSize = CircleSize.FinalValue.Value;
            difficulty.ApproachRate = ApproachRate.FinalValue.Value;
        }
    }
}
