// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModSuddenDeath : Mod
    {
        public override string Name => "Sudden Death";
        public override string ShortenedName => "SD";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_suddendeath;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Miss a note and fail.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModRelax), typeof(ModAutoplay) };
    }
}