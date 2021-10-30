using System;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class SettingsEnumPiece<T> : osu.Game.Screens.LLin.SideBar.Settings.Items.SettingsEnumPiece<T>
        where T : struct, Enum
    {
    }
}
