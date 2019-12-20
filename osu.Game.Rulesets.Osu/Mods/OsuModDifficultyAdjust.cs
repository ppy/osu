// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Drain Rate", "Override a beatmap's set HP.")]
        public override BindableNumber<float> DrainRate { get; } = new BindableFloat()
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Circle Size", "Override a beatmap's set CS.")]
        public override BindableNumber<float> CircleSize { get; } = new BindableFloat()
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Approach Rate", "Override a beatmap's set AR.")]
        public override BindableNumber<float> ApproachRate { get; } = new BindableFloat()
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };

        [SettingSource("Overall Difficulty", "Override a beatmap's set OD.")]
        public override BindableNumber<float> OverallDifficulty { get; } = new BindableFloat()
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
        };
    }
}