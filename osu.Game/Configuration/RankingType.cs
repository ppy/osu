using System;
namespace osu.Game.Configuration
{   
    public enum RankingType
    {
        Local,
        [DisplayName("Global")]
        Top,
        [DisplayName("Selected Mods")]
        SelectedMod,
        Friends,
        Country
    }
}