using Mvis.Plugin.FakeEditor.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.SideBar.Settings.Items;

namespace Mvis.Plugin.FakeEditor.UI
{
    public class FakeEditorSidebarSection : PluginSidebarSettingsSection
    {
        public FakeEditorSidebarSection(MvisPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (FakeEditorConfigManager)ConfigManager;

            AddRange(new Drawable[]
            {
                new SettingsTogglePiece
                {
                    Description = "启用Note打击音效",
                    Bindable = config.GetBindable<bool>(FakeEditorSetting.EnableFakeEditor)
                }
            });
        }
    }
}
