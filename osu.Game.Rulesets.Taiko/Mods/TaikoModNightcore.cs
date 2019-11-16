// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModNightcore : ModNightcore
    {
        public override string Description => @"动次打次动次打次~";
        public override double ScoreMultiplier => 1.12;
    }
}
