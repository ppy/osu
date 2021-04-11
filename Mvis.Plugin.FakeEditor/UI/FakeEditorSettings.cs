using Mvis.Plugin.FakeEditor.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace Mvis.Plugin.FakeEditor.UI
{
    public class FakeEditorSettings : PluginSettingsSubSection
    {
        protected override string Header => "谱面编辑器";

        public FakeEditorSettings(MvisPlugin plugin)
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
                    LabelText = "启用Note打击音效",
                    Current = config.GetBindable<bool>(FakeEditorSetting.EnableFakeEditor)
                },
            };
        }
    }
}
