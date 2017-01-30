//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using osu.Game.Graphics;

namespace osu.Game.Modes
{
    public class Mod
    {
        public Mods Name;

        public FontAwesome Icon;

        public double ScoreMultiplier;
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
