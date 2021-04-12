using Mvis.Plugin.StoryboardSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace Mvis.Plugin.StoryboardSupport.UI
{
    public class StoryboardSettings : PluginSettingsSubSection
    {
        public StoryboardSettings(MvisPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SbLoaderConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用故事版",
                    Current = config.GetBindable<bool>(SbLoaderSettings.EnableStoryboard)
                },
            };
        }
    }
}
