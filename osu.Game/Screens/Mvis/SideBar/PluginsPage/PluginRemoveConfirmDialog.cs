using System;

namespace osu.Game.Screens.Mvis.SideBar.PluginsPage
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class PluginRemoveConfirmDialog : osu.Game.Screens.LLin.SideBar.PluginsPage.PluginRemoveConfirmDialog
    {
        public PluginRemoveConfirmDialog(string headerText, Action<bool> onConfirm)
            : base(headerText, onConfirm)
        {
        }
    }
}
