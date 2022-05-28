using System;
using osu.Framework.Allocation;
using osu.Game.Screens.LLin.SideBar.Settings.Sections;

namespace osu.Game.Screens.LLin.Plugins.Config
{
    [Obsolete("请使用GetSettingEntries")]
    public abstract class PluginSidebarSettingsSection : Section
    {
        private readonly LLinPlugin plugin;
        protected IPluginConfigManager ConfigManager;

        protected PluginSidebarSettingsSection(LLinPlugin plugin)
        {
            this.plugin = plugin;
            Title = plugin.Name;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            ConfigManager = dependencies.Get<LLinPluginManager>().GetConfigManager(plugin);
            return dependencies;
        }
    }

    public class NewPluginSettingsSection : Section
    {
        private readonly LLinPlugin plugin;

        public NewPluginSettingsSection(LLinPlugin plugin)
        {
            this.plugin = plugin;
            Title = plugin.Name;

            Name = $"{plugin}的section";
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var se in plugin.GetSettingEntries())
            {
                var item = se.ToLLinSettingsItem();
                if (item != null) Add(se.ToLLinSettingsItem());
            }
        }
    }
}
