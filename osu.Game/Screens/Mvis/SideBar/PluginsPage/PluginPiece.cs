using System;
using LLinPlugin = osu.Game.Screens.LLin.Plugins.LLinPlugin;

namespace osu.Game.Screens.Mvis.SideBar.PluginsPage
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class PluginPiece : osu.Game.Screens.LLin.SideBar.PluginsPage.PluginPiece
    {
        public PluginPiece(LLinPlugin pl)
            : base(pl)
        {
        }
    }
}
