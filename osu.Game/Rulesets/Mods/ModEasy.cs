// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasy : Mod
    {
        public override string Name => "Easy";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Reduces overall difficulty - larger circles, more forgiving HP drain, less accuracy required.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };
    }
}