using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Mvis.Plugins.Config
{
    public abstract class PluginSettingsSubSection : SettingsSubsection
    {
        private readonly MvisPlugin plugin;
        protected IPluginConfigManager ConfigManager;

        protected override LocalisableString Header => plugin.Name;

        protected PluginSettingsSubSection(MvisPlugin plugin)
        {
            this.plugin = plugin;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            ConfigManager = dependencies.Get<MvisPluginManager>().GetConfigManager(plugin);
            return dependencies;
        }
    }
}
