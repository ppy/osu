// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using osu.Game.Graphics;

namespace osu.Game.Modes
{
    /// <summary>
    /// The base class for gameplay modifiers.
    /// </summary>
    public abstract class Mod
    {
        /// <summary>
        /// The name of this mod.
        /// </summary>
        public abstract Mods Name { get; }

        /// <summary>
        /// The icon of this mod.
        /// </summary>
        public abstract FontAwesome Icon { get; }

        /// <summary>
        /// The user readable description of this mod for the given <see cref="PlayMode"/>.
        /// </summary>
        public virtual string Description(PlayMode mode) => @"";

        /// <summary>
        /// The score multiplier of this mod for the given <see cref="PlayMode"/>
        /// </summary>
        public abstract double ScoreMultiplier(PlayMode mode);

        /// <summary>
        /// Returns if this mod is ranked in the given <see cref="PlayMode">
        /// </summary>
        public abstract bool Ranked(PlayMode mode);

        /// <summary>
        /// The mods this mod cannot be enabled with
        /// </summary>
        public abstract Mods[] DisablesMods(PlayMode mode);
    }

    public abstract class KeyMod : Mod
    {
        public abstract int KeyCount { get; }
        public override FontAwesome Icon => FontAwesome.fa_close; // TODO: Add proper key icons
        public override string Description(PlayMode mode) => @"";
        public override double ScoreMultiplier(PlayMode mode) => 1; // TODO: Implement the mania key mod score multiplier
        public override bool Ranked(PlayMode mode) => mode == PlayMode.Mania;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] {};
    }

    public class ModNoFail : Mod
    {
        public override Mods Name => Mods.NoFail;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Description(PlayMode mode) => @"You can't fail, no matter what.";
        public override double ScoreMultiplier(PlayMode mode) => 0.5;
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.Relax, Mods.Autopilot, Mods.SuddenDeath, Mods.Perfect };
    }

    public class ModEasy : Mod
    {
        public override Mods Name => Mods.Easy;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override string Description(PlayMode mode) => @"Reduces overall difficulty - larger circles, more forgiving HP drain, less accuracy required.";
        public override double ScoreMultiplier(PlayMode mode) => 0.5;
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.HardRock };
    }

    public class ModHidden : Mod
    {
        public override Mods Name => Mods.Hidden;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override string Description(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Osu:
                case PlayMode.Catch:
                    return @"Play with no approach circles and fading notes for a slight score advantage.";

                case PlayMode.Taiko:
                case PlayMode.Mania:
                    return @"The notes fade out before you hit them!";

                default:
                    throw new NotSupportedException();
            }
        }
        public override double ScoreMultiplier(PlayMode mode) => mode == PlayMode.Mania ? 1.0 : 1.06;
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => (mode == PlayMode.Mania) ? new Mods[] { Mods.Flashlight } : new Mods[] { };
    }

    public class ModHardRock : Mod
    {
        public override Mods Name => Mods.HardRock;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hardrock;
        public override string Description(PlayMode mode) => @"Everything just got a bit harder...";
        public override double ScoreMultiplier(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Osu:
                case PlayMode.Taiko:
                    return 1.06;

                case PlayMode.Catch:
                    return 1.12;

                case PlayMode.Mania:
                    return 1.0;

                default:
                    throw new NotSupportedException();
            }
        }
        public override bool Ranked(PlayMode mode) => mode != PlayMode.Mania;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.Easy };
    }

    public class ModSuddenDeath : Mod
    {
        public override Mods Name => Mods.SuddenDeath;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_suddendeath;
        public override string Description(PlayMode mode) => @"Miss a note and fail.";
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.NoFail, Mods.Relax, Mods.Autopilot, Mods.Autoplay, Mods.Cinema };
    }

    public class ModDoubleTime : Mod
    {
        public override Mods Name => Mods.DoubleTime;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_doubletime;
        public override string Description(PlayMode mode) => @"Zoooooooooom";
        public override double ScoreMultiplier(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Osu:
                case PlayMode.Taiko:
                    return 1.12;

                case PlayMode.Catch:
                    return 1.06;

                case PlayMode.Mania:
                    return 1.0;

                default:
                    throw new NotSupportedException();
            }
        }
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.HalfTime };
    }

    public class ModRelax : Mod
    {
        public override Mods Name => Mods.Relax;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override string Description(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Osu:
                    return "You don't need to click.\nGive your clicking/tapping finger a break from the heat of things.";
                    
                case PlayMode.Taiko:
                    return @"Relax! You will no longer get dizzyfied by ninja-like spinners, demanding drumrolls or unexpected katu's.";

                case PlayMode.Catch:
                    return @"Use the mouse to control the catcher.";

                case PlayMode.Mania:
                    return @"Unsupported";

                default:
                    throw new NotSupportedException();
            }
        }
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.Autopilot, Mods.Autoplay, Mods.Cinema, Mods.NoFail, Mods.SuddenDeath, Mods.Perfect };
    }

    public class ModHalfTime : Mod
    {
        public override Mods Name => Mods.HalfTime;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override string Description(PlayMode mode) => @"Less zoom";
        public override double ScoreMultiplier(PlayMode mode) => mode == PlayMode.Mania ? 0.5 : 0.3;
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.DoubleTime, Mods.Nightcore };
    }

    public class ModNightcore : ModDoubleTime
    {
        public override Mods Name => Mods.Nightcore;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nightcore;
        public override string Description(PlayMode mode) => @"uguuuuuuuu";
    }

    public class ModFlashlight : Mod
    {
        public override Mods Name => Mods.Flashlight;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override string Description(PlayMode mode) => @"Restricted view area.";
        public override double ScoreMultiplier(PlayMode mode) => mode == PlayMode.Mania ? 1.0 : 1.12;
        public override bool Ranked(PlayMode mode) => true;
        public override Mods[] DisablesMods(PlayMode mode) => (mode == PlayMode.Mania) ? new Mods[] { Mods.Hidden } : new Mods[] {};
    }

    public class ModAutoplay : Mod
    {
        public override Mods Name => Mods.Autoplay;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_auto;
        public override string Description(PlayMode mode) => @"Watch a perfect automated play through the song";
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.Relax, Mods.Autopilot, Mods.SpunOut, Mods.SuddenDeath, Mods.Perfect };
    }

    public class ModSpunOut : Mod
    {
        public override Mods Name => Mods.SpunOut;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override string Description(PlayMode mode) => @"Spinners will be automatically completed";
        public override double ScoreMultiplier(PlayMode mode) => 0.9;
        public override bool Ranked(PlayMode mode) => mode == PlayMode.Osu;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.Autoplay, Mods.Cinema, Mods.Autopilot };
    }

    public class ModAutopilot : Mod
    {
        public override Mods Name => Mods.Autopilot;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_autopilot;
        public override string Description(PlayMode mode) => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier(PlayMode mode) => 0;
        public override bool Ranked(PlayMode mode) => false;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.SpunOut, Mods.Relax, Mods.SuddenDeath, Mods.Perfect, Mods.NoFail, Mods.Autoplay, Mods.Cinema };
    }

    public class ModPerfect : ModSuddenDeath
    {
        public override Mods Name => Mods.Perfect;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_perfect;
        public override string Description(PlayMode mode) => @"SS or quit.";
    }

    public class ModKey4 : KeyMod
    {
        public override int KeyCount => 4;
        public override Mods Name => Mods.Key4;
    }

    public class ModKey5 : KeyMod
    {
        public override int KeyCount => 5;
        public override Mods Name => Mods.Key5;
    }

    public class ModKey6 : KeyMod
    {
        public override int KeyCount => 6;
        public override Mods Name => Mods.Key6;
    }

    public class ModKey7 : KeyMod
    {
        public override int KeyCount => 7;
        public override Mods Name => Mods.Key7;
    }

    public class ModKey8 : KeyMod
    {
        public override int KeyCount => 8;
        public override Mods Name => Mods.Key8;
    }

    public class ModFadeIn : Mod
    {
        public override Mods Name => Mods.FadeIn;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => mode == PlayMode.Mania;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] { Mods.Flashlight };
    }

    public class ModRandom : Mod
    {
        public override Mods Name => Mods.Random;
        public override FontAwesome Icon => FontAwesome.fa_close;
        public override string Description(PlayMode mode) => @"Shuffle around the notes!";
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => false;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] {};
    }

    public class ModCinema : ModAutoplay
    {
        public override Mods Name => Mods.Cinema;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_cinema;
    }

    public class ModTarget : Mod
    {
        public override Mods Name => Mods.Target;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => mode == PlayMode.Osu;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] {};
    }

    public class ModKey9 : KeyMod
    {
        public override int KeyCount => 9;
        public override Mods Name => Mods.Key9;
    }

    public class ModKeyCoop : Mod
    {
        public override Mods Name => Mods.KeyCoop;
        public override FontAwesome Icon => FontAwesome.fa_close;
        public override string Description(PlayMode mode) => @"Double the key amount, double the fun!";
        public override double ScoreMultiplier(PlayMode mode) => 1;
        public override bool Ranked(PlayMode mode) => mode == PlayMode.Mania;
        public override Mods[] DisablesMods(PlayMode mode) => new Mods[] {};
    }

    public class ModKey1 : KeyMod
    {
        public override int KeyCount => 1;
        public override Mods Name => Mods.Key1;
    }

    public class ModKey3 : KeyMod
    {
        public override int KeyCount => 3;
        public override Mods Name => Mods.Key3;
    }

    public class ModKey2 : KeyMod
    {
        public override int KeyCount => 2;
        public override Mods Name => Mods.Key2;
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
        Autopilot = 1 << 13,

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
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Autopilot | SpunOut | KeyMod,
        ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
    }
}