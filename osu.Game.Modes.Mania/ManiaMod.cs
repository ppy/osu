// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Modes.Mania
{
    public class ManiaModNoFail : ModNoFail
    {

    }

    public class ManiaModEasy : ModEasy
    {

    }

    public class ManiaModHidden : ModHidden
    {
        public override string Description => @"The notes fade out before you hit them!";
        public override double ScoreMultiplier => 1.0;
        public override Mods[] DisablesMods => new Mods[] { Mods.Flashlight };
    }

    public class ManiaModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.0;
        public override bool Ranked => false;
    }

    public class ManiaModSuddenDeath : ModSuddenDeath
    {

    }

    public class ManiaModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.0;
    }

    public class ManiaModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.3;
    }

    public class ManiaModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.0;
    }

    public class ManiaModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.0;
        public override Mods[] DisablesMods => new Mods[] { Mods.Hidden };
    }

    public class ManiaModPerfect : ModPerfect
    {

    }

    public class ManiaModFadeIn : Mod
    {
        public override Mods Name => Mods.FadeIn;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override string Description => @"";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.Flashlight };
    }

    public class ManiaModRandom : Mod
    {
        public override Mods Name => Mods.Random;
        public override FontAwesome Icon => FontAwesome.fa_close;
        public override string Description => @"Shuffle around the notes!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => false;
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public abstract class ManiaKeyMod : Mod
    {
        public abstract int KeyCount { get; }
        public override FontAwesome Icon => FontAwesome.fa_close; // TODO: Add proper key icons
        public override string Description => @"";
        public override double ScoreMultiplier => 1; // TODO: Implement the mania key mod score multiplier
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public class ManiaModKey1 : ManiaKeyMod
    {
        public override int KeyCount => 1;
        public override Mods Name => Mods.Key1;
    }

    public class ManiaModKey2 : ManiaKeyMod
    {
        public override int KeyCount => 2;
        public override Mods Name => Mods.Key2;
    }

    public class ManiaModKey3 : ManiaKeyMod
    {
        public override int KeyCount => 3;
        public override Mods Name => Mods.Key3;
    }

    public class ManiaModKey4 : ManiaKeyMod
    {
        public override int KeyCount => 4;
        public override Mods Name => Mods.Key4;
    }

    public class ManiaModKey5 : ManiaKeyMod
    {
        public override int KeyCount => 5;
        public override Mods Name => Mods.Key5;
    }

    public class ManiaModKey6 : ManiaKeyMod
    {
        public override int KeyCount => 6;
        public override Mods Name => Mods.Key6;
    }

    public class ManiaModKey7 : ManiaKeyMod
    {
        public override int KeyCount => 7;
        public override Mods Name => Mods.Key7;
    }

    public class ManiaModKey8 : ManiaKeyMod
    {
        public override int KeyCount => 8;
        public override Mods Name => Mods.Key8;
    }

    public class ManiaModKey9 : ManiaKeyMod
    {
        public override int KeyCount => 9;
        public override Mods Name => Mods.Key9;
    }

    public class ManiaModKeyCoop : Mod
    {
        public override Mods Name => Mods.KeyCoop;
        public override FontAwesome Icon => FontAwesome.fa_close;
        public override string Description => @"Double the key amount, double the fun!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { };
    }
}
