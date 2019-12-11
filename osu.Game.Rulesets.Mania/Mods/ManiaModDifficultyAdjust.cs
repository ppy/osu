// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Overall Difficulty", "Override the beatmap's set OD")]
        public override BindableNumber<float> OverallDifficulty { get; } = new BindableFloat
        {
            MinValue = 1,
            MaxValue = 10,
            Default = 5,
            Value = 5,
            Precision = 0.1F,
        };
    }
}
