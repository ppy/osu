using System;
using osu.Game.Screens.LLin.Plugins;

namespace osu.Game.Screens.Mvis.Plugins
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public abstract class PluginSidebarPage : osu.Game.Screens.LLin.Plugins.PluginSidebarPage
    {
        protected PluginSidebarPage(LLinPlugin plugin, float resizeWidth = -1)
            : base(plugin, resizeWidth)
        {
        }
    }
}
