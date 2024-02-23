// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHardRock : ModHardRock
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(TaikoModConstantSpeed) }).ToArray();
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        /// <summary>
        /// Multiplier factor added to the scrolling speed.
        /// </summary>
        /// <remarks>
        /// This factor is made up of two parts: the base part (1.4) and the aspect ratio adjustment (4/3).
        /// Stable applies the latter by dividing the width of the user's display by the width of a display with the same height, but 4:3 aspect ratio.
        /// TODO: Revisit if taiko playfield ever changes away from a hard-coded 16:9 (see https://github.com/ppy/osu/issues/5685).
        /// </remarks>
        private const double slider_multiplier = 1.4 * 4 / 3;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);
            difficulty.SliderMultiplier *= slider_multiplier;
        }
    }
}
