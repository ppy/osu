using System;
using System.ComponentModel;

namespace osu.Game.Configuration
{   
    public enum RankingType
    {
        Local,
        [Description("Global")]
        Top,
        [Description("Selected Mods")]
        SelectedMod,
        Friends,
        Country
    }
}