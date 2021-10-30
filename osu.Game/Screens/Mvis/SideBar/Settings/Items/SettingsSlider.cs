using System;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class SettingsSlider<T> : osu.Game.Screens.LLin.SideBar.Settings.Items.SettingsSlider<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
    }
}
