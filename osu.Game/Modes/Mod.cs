// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using osu.Game.Graphics;

namespace osu.Game.Modes
{
    public class Mod
    {
        public virtual Mods Name
        {
            get;
        }

        public virtual FontAwesome Icon
        {
            get;
        }

        public virtual double ScoreMultiplier(PlayMode mode)
        {
            return 1;
        }

        public virtual bool Ranked(PlayMode mode)
        {
            return false;
        }
    }

    public class ModNoFail : Mod
    {
        public override Mods Name => Mods.NoFail;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override double ScoreMultiplier(PlayMode mode) => 0.5;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModEasy : Mod
    {
        public override Mods Name => Mods.Easy;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override double ScoreMultiplier(PlayMode mode) => 0.5;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModHidden : Mod
    {
        public override Mods Name => Mods.Hidden;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override double ScoreMultiplier(PlayMode mode) => 1.06;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModHardRock : Mod
    {
        public override Mods Name => Mods.HardRock;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hardrock;
        public override double ScoreMultiplier(PlayMode mode) => 1.06;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModSuddenDeath : Mod
    {
        public override Mods Name => Mods.SuddenDeath;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_suddendeath;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModDoubleTime : Mod
    {
        public override Mods Name => Mods.DoubleTime;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_doubletime;
        public override double ScoreMultiplier(PlayMode mode) => 1.12;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModRelax : Mod
    {
        public override Mods Name => Mods.Relax;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
    }

    public class ModHalfTime : Mod
    {
        public override Mods Name => Mods.HalfTime;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override double ScoreMultiplier(PlayMode mode) => 0.3;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModNightcore : Mod
    {
        public override Mods Name => Mods.Nightcore;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nightcore;
        public override double ScoreMultiplier(PlayMode mode) => 1.12;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModFlashlight : Mod
    {
        public override Mods Name => Mods.Flashlight;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override double ScoreMultiplier(PlayMode mode) => 1.12;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModAutoplay : Mod
    {
        public override Mods Name => Mods.Autoplay;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_auto;
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
    }

    public class ModSpunOut : Mod
    {
        public override Mods Name => Mods.SpunOut;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override double ScoreMultiplier(PlayMode mode) => 0.9;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModRelax2 : Mod
    {
        public override Mods Name => Mods.Relax2;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_autopilot;
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
    }

    public class ModPerfect : Mod
    {
        public override Mods Name => Mods.Perfect;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_perfect;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey4 : Mod
    {
        public override Mods Name => Mods.Key4;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey5 : Mod
    {
        public override Mods Name => Mods.Key5;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey6 : Mod
    {
        public override Mods Name => Mods.Key6;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey7 : Mod
    {
        public override Mods Name => Mods.Key7;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey8 : Mod
    {
        public override Mods Name => Mods.Key8;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModFadeIn : Mod
    {
        public override Mods Name => Mods.FadeIn;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModRandom : Mod
    {
        public override Mods Name => Mods.Random;
        public override FontAwesome Icon => FontAwesome.fa_random;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => false;
    }

    public class ModCinema : Mod
    {
        public override Mods Name => Mods.Cinema;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_cinema;
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
    }

    public class ModTarget : Mod
    {
        public override Mods Name => Mods.Target;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey9 : Mod
    {
        public override Mods Name => Mods.Key9;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKeyCoop : Mod
    {
        public override Mods Name => Mods.KeyCoop;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey1 : Mod
    {
        public override Mods Name => Mods.Key1;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey3 : Mod
    {
        public override Mods Name => Mods.Key3;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }

    public class ModKey2 : Mod
    {
        public override Mods Name => Mods.Key2;
        public override FontAwesome Icon => FontAwesome.fa_key;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
    }


    [Flags]
    public enum Mods
    {
        None = 0,

        [Description(@"No Fail")]
        NoFail = 1 << 0,

        [Description(@"Easy")]
        Easy = 1 << 1,

        //NoVideo = 1 << 2,

        [Description(@"Hidden")]
        Hidden = 1 << 3,

        [Description(@"Hard Rock")]
        HardRock = 1 << 4,

        [Description(@"Sudden Death")]
        SuddenDeath = 1 << 5,

        [Description(@"Double Time")]
        DoubleTime = 1 << 6,

        [Description(@"Relax")]
        Relax = 1 << 7,

        [Description(@"Halftime")]
        HalfTime = 1 << 8,

        [Description(@"Nightcore")]
        Nightcore = 1 << 9,

        [Description(@"Flashlight")]
        Flashlight = 1 << 10,

        [Description(@"Auto")]
        Autoplay = 1 << 11,

        [Description(@"Spun Out")]
        SpunOut = 1 << 12,

        [Description(@"Autopilot")]
        Relax2 = 1 << 13,

        [Description(@"Perfect")]
        Perfect = 1 << 14,

        [Description(@"4K")]
        Key4 = 1 << 15,

        [Description(@"5K")]
        Key5 = 1 << 16,

        [Description(@"6K")]
        Key6 = 1 << 17,

        [Description(@"7K")]
        Key7 = 1 << 18,

        [Description(@"8K")]
        Key8 = 1 << 19,

        [Description(@"Fade In")]
        FadeIn = 1 << 20,

        [Description(@"Random")]
        Random = 1 << 21,

        [Description(@"Cinema")]
        Cinema = 1 << 22,

        [Description(@"Target Practice")]
        Target = 1 << 23,

        [Description(@"9K")]
        Key9 = 1 << 24,

        [Description(@"Co-Op")]
        KeyCoop = 1 << 25,

        [Description(@"1K")]
        Key1 = 1 << 26,

        [Description(@"3K")]
        Key3 = 1 << 27,

        [Description(@"2K")]
        Key2 = 1 << 28,

        LastMod = 1 << 29,

        KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
        ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
    }
}