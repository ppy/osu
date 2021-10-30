using M.Resources.Localisation.LLin;
using Mvis.Plugin.FakeEditor.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

namespace Mvis.Plugin.FakeEditor.UI
{
    public class FakeEditorSidebarSection : PluginSidebarSettingsSection
    {
        public FakeEditorSidebarSection(LLinPlugin plugin)
            : base(plugin)
        {
        }

        public override int Columns => 1;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (FakeEditorConfigManager)ConfigManager;

            AddRange(new Drawable[]
            {
                new SettingsTogglePiece
                {
                    Description = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(FakeEditorSetting.EnableFakeEditor)
                }
            });
        }
    }
}
