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
        /// The user readable description of this mod.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The score multiplier of this mod.
        /// </summary>
        public abstract double ScoreMultiplier { get; }

        /// <summary>
        /// Returns if this mod is ranked.
        /// </summary>
        public abstract bool Ranked { get; }

        /// <summary>
        /// The mods this mod cannot be enabled with.
        /// </summary>
        public abstract Mods[] DisablesMods { get; }
    }

    public class MultiMod : Mod
    {
        public override Mods Name => Modes.Mods.None;
        public override FontAwesome Icon => FontAwesome.fa_close;
        public override string Description => @"";
        public override double ScoreMultiplier => 0.0;
        public override bool Ranked => false;
        public override Mods[] DisablesMods => new Mods[] { };

        public Mod[] Mods;
    }

    public abstract class ModNoFail : Mod
    {
        public override Mods Name => Mods.NoFail;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Description => @"You can't fail, no matter what.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.Relax, Mods.Autopilot, Mods.SuddenDeath, Mods.Perfect };
    }

    public abstract class ModEasy : Mod
    {
        public override Mods Name => Mods.Easy;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override string Description => @"Reduces overall difficulty - larger circles, more forgiving HP drain, less accuracy required.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.HardRock };
    }

    public abstract class ModHidden : Mod
    {
        public override Mods Name => Mods.Hidden;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override bool Ranked => true;
    }

    public abstract class ModHardRock : Mod
    {
        public override Mods Name => Mods.HardRock;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hardrock;
        public override string Description => @"Everything just got a bit harder...";
        public override Mods[] DisablesMods => new Mods[] { Mods.Easy };
    }

    public abstract class ModSuddenDeath : Mod
    {
        public override Mods Name => Mods.SuddenDeath;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_suddendeath;
        public override string Description => @"Miss a note and fail.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.NoFail, Mods.Relax, Mods.Autopilot, Mods.Autoplay, Mods.Cinema };
    }

    public abstract class ModDoubleTime : Mod
    {
        public override Mods Name => Mods.DoubleTime;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_doubletime;
        public override string Description => @"Zoooooooooom";
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.HalfTime };
    }

    public abstract class ModRelax : Mod
    {
        public override Mods Name => Mods.Relax;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Mods[] DisablesMods => new Mods[] { Mods.Autopilot, Mods.Autoplay, Mods.Cinema, Mods.NoFail, Mods.SuddenDeath, Mods.Perfect };
    }

    public abstract class ModHalfTime : Mod
    {
        public override Mods Name => Mods.HalfTime;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override string Description => @"Less zoom";
        public override bool Ranked => true;
        public override Mods[] DisablesMods => new Mods[] { Mods.DoubleTime, Mods.Nightcore };
    }

    public abstract class ModNightcore : ModDoubleTime
    {
        public override Mods Name => Mods.Nightcore;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nightcore;
        public override string Description => @"uguuuuuuuu";
    }

    public abstract class ModFlashlight : Mod
    {
        public override Mods Name => Mods.Flashlight;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override string Description => @"Restricted view area.";
        public override bool Ranked => true;
    }

    public class ModAutoplay : Mod
    {
        public override Mods Name => Mods.Autoplay;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_auto;
        public override string Description => @"Watch a perfect automated play through the song";
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Mods[] DisablesMods => new Mods[] { Mods.Relax, Mods.Autopilot, Mods.SpunOut, Mods.SuddenDeath, Mods.Perfect };
    }

    public abstract class ModPerfect : ModSuddenDeath
    {
        public override Mods Name => Mods.Perfect;
        public override FontAwesome Icon => FontAwesome.fa_close;
        public override string Description => @"SS or quit.";
    }

    public class ModCinema : ModAutoplay
    {
        public override Mods Name => Mods.Cinema;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_cinema;
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

    public enum ModType
    {
        DifficultyReduction,
        DifficultyIncrease,
        Special,
    }
}
