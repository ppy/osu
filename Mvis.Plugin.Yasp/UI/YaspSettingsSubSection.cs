using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.Yasp.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.Yasp.UI
{
    public class YaspSettingsSubSection : PluginSettingsSubSection
    {
        public YaspSettingsSubSection(LLinPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (YaspConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = YaspStrings.Scale,
                    Current = config.GetBindable<float>(YaspSettings.Scale),
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = LLinGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(YaspSettings.EnablePlugin)
                }
            };
        }
    }
}
