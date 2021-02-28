using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class MvisPluginManager : Component
    {
        private readonly BindableList<MvisPlugin> avaliablePlugins = new BindableList<MvisPlugin>();
        private readonly BindableList<MvisPlugin> activePlugins = new BindableList<MvisPlugin>();

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private SideBar.Sidebar sideBar { get; set; }

        public bool AddPlugin(MvisPlugin pl)
        {
            if (avaliablePlugins.Contains(pl) || pl == null) return false;

            avaliablePlugins.Add(pl);
            sideBar?.Add(pl.SidebarPage);
            return true;
        }

        public bool UnLoadPlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || pl == null) return false;

            sideBar?.Remove(pl.SidebarPage);

            pl.Disable();

            pl.UnLoad();
            avaliablePlugins.Remove(pl);
            return true;
        }

        public bool ActivePlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || activePlugins.Contains(pl) || pl == null) return false;

            bool success = pl.Enable();

            if (success)
                activePlugins.Add(pl);

            return success;
        }

        public bool DisablePlugin(MvisPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || !activePlugins.Contains(pl) || pl == null) return false;

            bool success = pl.Disable();

            if (success)
                activePlugins.Remove(pl);

            return success;
        }

        public List<MvisPlugin> GetAvaliablePlugins()
        {
            return avaliablePlugins.ToList();
        }

        public List<MvisPlugin> GetActivePlugins()
        {
            return activePlugins.ToList();
        }
    }
}
