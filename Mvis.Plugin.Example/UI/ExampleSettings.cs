using Mvis.Plugin.Example.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.Example.UI
{
    public class ExampleSettings : PluginSettingsSubSection
    {
        public ExampleSettings(LLinPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (ExamplePluginConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Bool值的设置",
                    Current = config.GetBindable<bool>(ExamplePluginSettings.KeyBool)
                },
                new SettingsSlider<float>
                {
                    LabelText = "Float值的设置",
                    Current = config.GetBindable<float>(ExamplePluginSettings.KeyFloat)
                },
                new SettingsSlider<double>
                {
                    LabelText = "Double值的设置(以百分比显示)",
                    DisplayAsPercentage = true,
                    Current = config.GetBindable<double>(ExamplePluginSettings.KeyDouble)
                },
                new SettingsTextBox
                {
                    LabelText = "String值的设置",
                    RelativeSizeAxes = Axes.X,
                    Current = config.GetBindable<string>(ExamplePluginSettings.KeyString)
                },
                new SettingsEnumDropdown<ExampleEnum>
                {
                    LabelText = "Enum值的设置",
                    Current = config.GetBindable<ExampleEnum>(ExamplePluginSettings.keyEnum)
                }
            };
        }
    }
}
