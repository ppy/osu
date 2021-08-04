using M.Resources.Localisation.Mvis.Plugins;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class CollectionSupportEntryButton : PluginBottomBarButton
    {
        public CollectionSupportEntryButton(PluginSidebarPage page)
            : base(page)
        {
            ButtonIcon = FontAwesome.Solid.Check;
            TooltipText = CollectionStrings.EntryTooltip;
        }
    }
}
