using System;
using osu.Game.Screens.LLin.Plugins;

namespace osu.Game.Screens.Mvis.Plugins.Config
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public abstract class PluginSidebarSettingsSection : osu.Game.Screens.LLin.Plugins.Config.PluginSidebarSettingsSection
    {
        protected PluginSidebarSettingsSection(LLinPlugin plugin)
            : base(plugin)
        {
        }
    }
}
