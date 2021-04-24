using osu.Game.Screens.Mvis.BottomBar.Buttons;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class PluginBottomBarButton : BottomBarButton
    {
        public readonly PluginSidebarPage Page;

        public PluginBottomBarButton(PluginSidebarPage page)
        {
            Page = page;
        }
    }
}
