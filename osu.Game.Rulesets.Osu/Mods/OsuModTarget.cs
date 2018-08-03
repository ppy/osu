// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTarget : Mod
    {
        public override string Name => "Target";
        public override string ShortenedName => "TP";
        public override ModType Type => ModType.Conversion;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override string Description => @"Practice keeping up with the beat of the song.";
        public override double ScoreMultiplier => 1;
    }
}
