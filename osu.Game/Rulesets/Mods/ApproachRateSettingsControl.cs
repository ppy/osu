// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    public class ApproachRateSettingsControl : DifficultyAdjustSettingsControl
    {
        public ApproachRateSettingsControl()
        {
            CurrentNumber.Precision = 0.1f;

            CurrentNumber.MinValue = 0;
            CurrentNumber.MaxValue = 10;
        }

        protected override float UpdateFromDifficulty(BeatmapDifficulty difficulty) => difficulty.ApproachRate;
    }
}
