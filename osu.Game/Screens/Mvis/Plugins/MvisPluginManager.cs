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

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private SideBar.Sidebar sideBar { get; set; }

        public Action<MvisPlugin> OnPluginAdd;
        public Action<MvisPlugin> OnPluginUnLoad;

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

            activePlugins.Remove(pl);
            avaliablePlugins.Remove(pl);

            try
            {
                pl.UnLoad();
                OnPluginUnLoad?.Invoke(pl);
            }
            catch (Exception e)
            {
                Logger.Error(e, "卸载插件时出现了问题");
                avaliablePlugins.Add(pl);
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
        public List<MvisPlugin> GetAllPlugins() => avaliablePlugins.ToList();
    }
}
