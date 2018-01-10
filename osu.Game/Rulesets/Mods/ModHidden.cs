// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : Mod
    {
        public override string Name => "Hidden";
        public override string ShortenedName => "HD";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;
    }
}
