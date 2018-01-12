// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKeyCoop : Mod
    {
        public override string Name => "KeyCoop";
        public override string ShortenedName => "2P";
        public override string Description => @"Double the key amount, double the fun!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
    }
}
