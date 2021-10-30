using osu.Framework.Allocation;
using osu.Game.Screens.LLin.SideBar.Settings.Sections;

namespace osu.Game.Screens.LLin.Plugins.Config
{
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
}
