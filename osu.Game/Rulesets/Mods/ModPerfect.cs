// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPerfect : ModSuddenDeath
    {
        public override string Name => "Perfect";
        public override string ShortenedName => "PF";
        public override string Description => "SS or quit.";
    }
}