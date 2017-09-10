// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoFail : Mod
    {
        public override string Name => "NoFail";
        public override string ShortenedName => "NF";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "You can't fail, no matter what.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModAutoplay) };

        /// <summary>
        /// We never fail, 'yo.
        /// </summary>
        public override bool AllowFail => false;
    }
}