// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHalfTime : ModHalfTime
    {
        public override string Description => @"减<<<<<<速 ( 0.75倍速 ) ( 按下切换到Daycore )";
        public override double ScoreMultiplier => 0.3;
    }
}
