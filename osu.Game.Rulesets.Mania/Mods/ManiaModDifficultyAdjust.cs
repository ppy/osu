// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDifficultyAdjust : ModDifficultyAdjust
    {
        public override DifficultyBindable OverallDifficulty { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            ExtendedMaxValue = 13.61f,
            ExtendedMinValue = -14.93f,
            ReadCurrentFromDifficulty = diff => diff.OverallDifficulty,
        };
    }
}
