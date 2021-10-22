using System;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Screens.LLin.Plugins.Config
{
    public abstract class PluginConfigManager<TLookup> : IniConfigManager<TLookup>, IPluginConfigManager
        where TLookup : struct, Enum
    {
        protected abstract string ConfigName { get; }
        protected override string Filename => $"custom/plugin-{ConfigName}.ini";

        protected PluginConfigManager(Storage storage)
            : base(storage)
        {
        }
    }
}
