using System;
using osu.Game.Screens.LLin.Plugins;

namespace osu.Game.Screens.LLin.Misc.PluginResolvers
{
    [Obsolete("Mvis => LLin")]
    public class MvisPluginResolver : LLinPluginResolver
    {
        public MvisPluginResolver(LLinPluginManager pluginManager)
            : base(pluginManager)
        {
        }
    }
}
