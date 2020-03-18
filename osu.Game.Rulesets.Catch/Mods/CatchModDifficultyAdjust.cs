// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Circle Size", "Override a beatmap's set CS.", FIRST_SETTING_ORDER - 1)]
        public BindableNumber<float> CircleSize { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Approach Rate", "Override a beatmap's set AR.", LAST_SETTING_ORDER + 1)]
        public BindableNumber<float> ApproachRate { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        protected override void TransferSettings(BeatmapDifficulty difficulty)
        {
            base.TransferSettings(difficulty);

            TransferSetting(CircleSize, difficulty.CircleSize);
            TransferSetting(ApproachRate, difficulty.ApproachRate);
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            difficulty.CircleSize = CircleSize.Value;
            difficulty.ApproachRate = ApproachRate.Value;
        }
    }
}
