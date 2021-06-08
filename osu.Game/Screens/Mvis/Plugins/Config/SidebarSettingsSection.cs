using osu.Framework.Allocation;
using osu.Game.Screens.Mvis.SideBar.Settings.Sections;

namespace osu.Game.Screens.Mvis.Plugins.Config
{
    public abstract class PluginSidebarSettingsSection : Section
    {
        private readonly MvisPlugin plugin;
        protected IPluginConfigManager ConfigManager;

        protected PluginSidebarSettingsSection(MvisPlugin plugin)
        {
            this.plugin = plugin;
            Title = plugin.Name;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            ConfigManager = dependencies.Get<MvisPluginManager>().GetConfigManager(plugin);
            return dependencies;
        }
    }
}
