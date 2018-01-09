// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRelax : Mod
    {
        public override string Name => "Relax";
        public override string ShortenedName => "RX";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override double ScoreMultiplier => 0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModNoFail), typeof(ModSuddenDeath) };
    }
}
