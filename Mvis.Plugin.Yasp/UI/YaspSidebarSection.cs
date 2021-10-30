using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.Yasp.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

namespace Mvis.Plugin.Yasp.UI
{
    public class YaspSidebarSection : PluginSidebarSettingsSection
    {
        public YaspSidebarSection(LLinPlugin plugin)
            : base(plugin)
        {
        }

        public override int Columns => 2;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (YaspConfigManager)ConfigManager;

            AddRange(new Drawable[]
            {
                new SettingsTogglePiece
                {
                    Description = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(YaspSettings.EnablePlugin)
                },
                new SettingsSliderPiece<float>
                {
                    Icon = FontAwesome.Solid.ExpandArrowsAlt,
                    Description = YaspStrings.Scale,
                    Bindable = config.GetBindable<float>(YaspSettings.Scale),
                    DisplayAsPercentage = true
                }
            });
        }
    }
}
