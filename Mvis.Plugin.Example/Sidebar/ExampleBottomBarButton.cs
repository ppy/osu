using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.Example.Sidebar
{
    public class ExampleBottomBarButton : PluginBottomBarButton
    {
        public ExampleBottomBarButton(PluginSidebarPage page) : base(page)
        {
            ButtonIcon = FontAwesome.Solid.Egg;
            TooltipText = "Hi!";
        }
    }
}
