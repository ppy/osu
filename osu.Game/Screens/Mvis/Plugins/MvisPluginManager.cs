using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class MvisPluginManager : Component
    {
        private readonly BindableList<MvisPlugin> avaliablePlugins = new BindableList<MvisPlugin>();
        private readonly BindableList<MvisPlugin> activePlugins = new BindableList<MvisPlugin>();
        private readonly List<MvisPluginProvider> providers = new List<MvisPluginProvider>();

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private SideBar.Sidebar sideBar { get; set; }

        public Action<MvisPlugin> OnPluginAdd;
        public Action<MvisPlugin> OnPluginUnLoad;

        [BackgroundDependencyLoader]
        private void load(CustomStore customStore)
        {
            foreach (var provider in customStore.LoadedPluginProviders)
            {
                AddPlugin(provider.CreatePlugin);
                providers.Add(provider);
            }
        }

        public bool AddPlugin(MvisPlugin pl)
        {
            if (avaliablePlugins.Contains(pl) || pl == null) return false;

            avaliablePlugins.Add(pl);
            sideBar?.Add(pl.SidebarPage);
            OnPluginAdd?.Invoke(pl);
            return true;
        }

        public bool UnLoadPlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || pl == null) return false;

            sideBar?.Remove(pl.SidebarPage);

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
                Logger.Error(e, "卸载插件时出现了问题");
                avaliablePlugins.Add(pl);
                providers.Add(provider);
                throw;
            }

            return true;
        }

        public bool ActivePlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || activePlugins.Contains(pl) || pl == null) return false;

            if (!activePlugins.Contains(pl))
                activePlugins.Add(pl);

            bool success = pl.Enable();

            if (!success)
                activePlugins.Remove(pl);

            return success;
        }

        public bool DisablePlugin(MvisPlugin pl)
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
        /// <param name="newInstance">是否创建新插件本体，该选项应当只用于MvisScreen启动！</param>
        /// <returns>所有已加载且可用的插件</returns>
        public List<MvisPlugin> GetAllPlugins(bool newInstance)
        {
            if (newInstance)
            {
                DisposeAllPlugins();

                foreach (var p in providers)
                {
                    avaliablePlugins.Add(p.CreatePlugin);
                }
            }

            return avaliablePlugins.ToList();
        }

        public void DisposeAllPlugins()
        {
            foreach (var pl in avaliablePlugins)
            {
                activePlugins.Remove(pl);
                pl.Dispose();
            }

            avaliablePlugins.Clear();
        }
    }
}
