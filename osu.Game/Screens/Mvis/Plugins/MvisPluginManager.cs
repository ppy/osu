using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class MvisPluginManager : Component
    {
        private readonly BindableList<MvisPlugin> avaliablePlugins = new BindableList<MvisPlugin>();
        private readonly BindableList<MvisPlugin> activePlugins = new BindableList<MvisPlugin>();
        private readonly List<MvisPluginProvider> providers = new List<MvisPluginProvider>();
        private readonly ConcurrentDictionary<Type, IPluginConfigManager> configManagers = new ConcurrentDictionary<Type, IPluginConfigManager>();

        [Resolved]
        private Storage storage { get; set; }

        internal Action<MvisPlugin> OnPluginAdd;
        internal Action<MvisPlugin> OnPluginUnLoad;

        public int PluginVersion => 3;
        public int MinimumPluginVersion => 2;
        private const bool experimental = false;

        [BackgroundDependencyLoader]
        private void load(CustomStore customStore)
        {
            foreach (var provider in customStore.LoadedPluginProviders)
            {
                AddPlugin(provider.CreatePlugin);
                providers.Add(provider);
            }

            if (!DebugUtils.IsDebugBuild && experimental)
            {
                Logger.Log($"看上去该版本 ({PluginVersion}) 尚处于实现性阶段。 "
                           + "请留意该版本的任何功能都可能会随时变动。 ",
                    LoggingTarget.Runtime,
                    LogLevel.Important);
            }
        }

        public IPluginConfigManager GetConfigManager(MvisPlugin pl) =>
            configManagers.GetOrAdd(pl.GetType(), _ => pl.CreateConfigManager(storage));

        internal bool AddPlugin(MvisPlugin pl)
        {
            if (avaliablePlugins.Contains(pl) || pl == null) return false;

            if (pl.Version < MinimumPluginVersion)
                Logger.Log($"插件 \"{pl.Name}\" 是为旧版本的mf-osu打造的, 继续使用可能会导致意外情况的发生!", LoggingTarget.Runtime, LogLevel.Important);
            else if (pl.Version > PluginVersion)
                Logger.Log($"插件 \"{pl.Name}\" 是为更高版本的mf-osu打造的, 继续使用可能会导致意外情况的发生!", LoggingTarget.Runtime, LogLevel.Important);

            avaliablePlugins.Add(pl);
            OnPluginAdd?.Invoke(pl);
            return true;
        }

        internal bool UnLoadPlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || pl == null) return false;

            var provider = providers.Find(p => p.CreatePlugin.GetType() == pl.GetType());

            activePlugins.Remove(pl);
            avaliablePlugins.Remove(pl);
            providers.Remove(provider);

            try
            {
                pl.UnLoad();
                OnPluginUnLoad?.Invoke(pl);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"卸载插件时出现了问题: {e.Message}");

                //直接dispose掉插件
                if (pl.Parent is Container container)
                {
                    container.Remove(pl);
                    pl.Dispose();
                }
            }

            return true;
        }

        internal bool ActivePlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || activePlugins.Contains(pl) || pl == null) return false;

            if (!activePlugins.Contains(pl))
                activePlugins.Add(pl);

            bool success = pl.Enable();

            if (!success)
                activePlugins.Remove(pl);

            return success;
        }

        internal bool DisablePlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || !activePlugins.Contains(pl) || pl == null) return false;

            activePlugins.Remove(pl);
            bool success = pl.Disable();

            if (!success)
            {
                activePlugins.Add(pl);
                Logger.Log($"卸载插件\"${pl.Name}\"失败");
            }

            return success;
        }

        public List<MvisPlugin> GetActivePlugins() => activePlugins.ToList();

        /// <summary>
        /// 获取所有插件
        /// </summary>
        /// <param name="newInstance">
        /// 是否处理当前所有插件并创建新插件本体<br/>
        /// </param>
        /// <returns>所有已加载且可用的插件</returns>
        public List<MvisPlugin> GetAllPlugins(bool newInstance)
        {
            if (newInstance)
            {
                //bug: 直接调用Dispose会导致快速进出时抛出Disposed drawabled may never in the scene graph
                ExpireOldPlugins();

                foreach (var p in providers)
                {
                    avaliablePlugins.Add(p.CreatePlugin);
                }
            }

            return avaliablePlugins.ToList();
        }

        internal void ExpireOldPlugins()
        {
            foreach (var pl in avaliablePlugins)
            {
                activePlugins.Remove(pl);
                pl.Expire();
            }

            avaliablePlugins.Clear();
        }
    }
}
