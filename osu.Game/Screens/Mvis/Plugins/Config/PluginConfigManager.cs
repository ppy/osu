using System;
using osu.Framework.Platform;

namespace osu.Game.Screens.Mvis.Plugins.Config
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public abstract class PluginConfigManager<TLookup> : osu.Game.Screens.LLin.Plugins.Config.PluginConfigManager<TLookup>
        where TLookup : struct, Enum
    {
        protected PluginConfigManager(Storage storage)
            : base(storage)
        {
        }
    }
}
