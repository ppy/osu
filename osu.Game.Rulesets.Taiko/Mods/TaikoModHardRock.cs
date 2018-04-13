﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;
    }
}
