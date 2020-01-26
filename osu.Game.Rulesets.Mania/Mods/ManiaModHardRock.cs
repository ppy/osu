// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            var baseKeyAmount = difficulty.CircleSize;
            base.ApplyToDifficulty(difficulty);
            difficulty.CircleSize = baseKeyAmount;
        }
    }
}
