// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFlashlight : Mod
    {
        public override string Name => "Flashlight";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override string Description => "Restricted view area.";
        public override bool Ranked => true;
    }
}