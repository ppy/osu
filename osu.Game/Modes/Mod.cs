//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Graphics;

namespace osu.Game.Modes
{
    public class Mod
    {
        public ModName Name;
        public ModType Type;

        public FontAwesome Icon;

        public double ScoreMultiplier;
    }

    public enum ModType
    {
        DiffReducing,
        DiffIncreasing,
        Assistance
    }
    public enum ModName
    {
        [Description(@"Autopilot")]
        AP,
        [Description(@"Auto")]
        AU,
        [Description(@"Cinema")]
        CM,
        [Description(@"Double Time")]
        DT,
        [Description(@"Easy")]
        EZ,
        [Description(@"Flashlight")]
        FL, 
        [Description(@"Halftime")]
        HT, 
        [Description(@"Hard Rock")]
        HR, 
        [Description(@"Hidden")]
        HD, 
        [Description(@"Nightcore")]
        NC, 
        [Description(@"No Fail")]
        NF, 
        [Description(@"Perfect")]
        PF, 
        [Description(@"Relax")]
        RX, 
        [Description(@"Spun Out")]
        SO, 
        [Description(@"Sudden Death")]
        SD, 
        [Description(@"Target Practice")]
        TP  
    }
}
