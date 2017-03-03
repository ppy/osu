// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Modes.Osu
{
    public class OsuModNoFail : ModNoFail
    {
        
    }

    public class OsuModEasy : ModEasy
    {

    }

    public class OsuModHidden : ModHidden
    {
        public override string Description => @"Play with no approach circles and fading notes for a slight score advantage.";
        public override double ScoreMultiplier => 1.06;
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public class OsuModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;
    }

    public class OsuModSuddenDeath : ModSuddenDeath
    {
        
    }

    public class OsuModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModRelax : ModRelax
    {
        public override string Description => "You don't need to click.\nGive your clicking/tapping finger a break from the heat of things.";
    }

    public class OsuModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class OsuModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.12;
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public class OsuModPerfect : ModPerfect
    {
        
    }

    public class OsuModSpunOut : Mod
    {
        public override Mods Name => Mods.SpunOut;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override string Description => @"Spinners will be automatically completed";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.Autoplay, Mods.Cinema, Mods.Autopilot };
    }

    public class OsuModAutopilot : Mod
    {
        public override Mods Name => Mods.Autopilot;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_autopilot;
        public override string Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Mods[] DisablesMods => new Mods[] { Mods.SpunOut, Mods.Relax, Mods.SuddenDeath, Mods.Perfect, Mods.NoFail, Mods.Autoplay, Mods.Cinema };
    }

    public class OsuModTarget : Mod
    {
        public override Mods Name => Mods.Target;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override string Description => @"";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => false;
        public override Mods[] DisablesMods => new Mods[] { };
    }
}
