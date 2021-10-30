using M.Resources.Localisation.LLin;
using Mvis.Plugin.StoryboardSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.StoryboardSupport.UI
{
    public class StoryboardSettings : PluginSettingsSubSection
    {
        public StoryboardSettings(LLinPlugin plugin)
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
                    LabelText = LLinGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(SbLoaderSettings.EnableStoryboard)
                },
            };
        }
    }
}
