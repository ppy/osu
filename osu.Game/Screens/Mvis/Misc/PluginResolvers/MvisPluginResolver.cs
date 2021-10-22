using System;
using osu.Game.Screens.LLin.Plugins;

namespace osu.Game.Screens.Mvis.Misc.PluginResolvers
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class MvisPluginResolver : osu.Game.Screens.LLin.Misc.PluginResolvers.MvisPluginResolver
    {
        public MvisPluginResolver(LLinPluginManager pluginManager)
            : base(pluginManager)
        {
        }
    }
}
