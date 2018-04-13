// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mods
{
    public class MultiMod : Mod
    {
        public override string Name => string.Empty;
        public override string ShortenedName => string.Empty;
        public override string Description => string.Empty;
        public override double ScoreMultiplier => 0;

        public Mod[] Mods;
    }
}
