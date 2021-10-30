using M.Resources.Localisation.LLin;
using Mvis.Plugin.FakeEditor.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.FakeEditor.UI
{
    public class FakeEditorSettings : PluginSettingsSubSection
    {
        public FakeEditorSettings(LLinPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (FakeEditorConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = LLinGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(FakeEditorSetting.EnableFakeEditor)
                },
            };
        }
    }
}
