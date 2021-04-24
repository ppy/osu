using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class CollectionSupportEntryButton : PluginBottomBarButton
    {
        public CollectionSupportEntryButton(PluginSidebarPage page)
            : base(page)
        {
            ButtonIcon = FontAwesome.Solid.List;
            TooltipText = "查看收藏夹";
        }
    }
}
