// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModEasy : ModEasy
    {
        public override LocalisableString Description => @"音符移动更慢,更低的准确率要求!";

        /// <summary>
        /// Multiplier factor added to the scrolling speed.
        /// </summary>
        private const double slider_multiplier = 0.8;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);
            difficulty.SliderMultiplier *= slider_multiplier;
        }
    }
}
