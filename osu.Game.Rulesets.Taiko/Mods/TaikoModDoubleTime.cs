// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModDoubleTime : ModDoubleTime
    {
        public override string Description => @"加>>>>>>>>>速 ( 1.5倍速 ) ( 按下切换到NightCore )";
        public override double ScoreMultiplier => 1.12;
    }
}
