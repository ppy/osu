// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFlashlight : Mod
    {
        public override string Name => "Flashlight";
        public override string ShortenedName => "FL";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Restricted view area.";
        public override bool Ranked => true;
    }
}
