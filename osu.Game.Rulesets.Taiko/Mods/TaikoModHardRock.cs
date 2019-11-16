// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHardRock : ModHardRock
    {
        public override string Description => @"在各方面都难一点";
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;
    }
}
