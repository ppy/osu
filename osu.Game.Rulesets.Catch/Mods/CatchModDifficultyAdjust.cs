// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Circle Size", "Override a beatmap's set CS.", FIRST_SETTING_ORDER - 1)]
        public DifficultyTrackingBindable<float> CircleSize { get; } = new DifficultyTrackingBindable<float>
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
        };

        [SettingSource("Approach Rate", "Override a beatmap's set AR.", LAST_SETTING_ORDER + 1)]
        public DifficultyTrackingBindable<float> ApproachRate { get; } = new DifficultyTrackingBindable<float>
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
        };

        public override string SettingDescription
        {
            get
            {
                string circleSize = CircleSize.IsDefault ? string.Empty : $"CS {CircleSize.Value:N1}";
                string approachRate = ApproachRate.IsDefault ? string.Empty : $"AR {ApproachRate.Value:N1}";

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

            CircleSize.ChangeBase(difficulty.CircleSize);
            ApproachRate.ChangeBase(difficulty.ApproachRate);
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            difficulty.CircleSize = CircleSize.Value;
            difficulty.ApproachRate = ApproachRate.Value;
        }
    }
}
