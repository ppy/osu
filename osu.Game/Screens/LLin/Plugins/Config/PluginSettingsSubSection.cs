using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.LLin.Plugins.Config
{
    [Obsolete("请使用GetSettingEntries")]
    public abstract class PluginSettingsSubSection : SettingsSubsection
    {
        private readonly LLinPlugin plugin;
        protected IPluginConfigManager ConfigManager;

        protected override LocalisableString Header => plugin.Name;

        protected PluginSettingsSubSection(LLinPlugin plugin)
        {
            this.plugin = plugin;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            ConfigManager = dependencies.Get<LLinPluginManager>().GetConfigManager(plugin);
            return dependencies;
        }
    }

    public class PluginSettingsSubsection : SettingsSubsection
    {
        private readonly LLinPlugin plugin;

        public PluginSettingsSubsection(LLinPlugin plugin)
        {
            this.plugin = plugin;
            Name = $"{plugin}的subsection";

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
        }

        protected override LocalisableString Header => plugin.Name;

        [BackgroundDependencyLoader]
        private void load(LLinPluginManager pluginManager)
        {
            foreach (var se in pluginManager.GetSettingsFor(plugin))
            {
                Add(se.ToSettingsItem());
            }
        }
    }
}
