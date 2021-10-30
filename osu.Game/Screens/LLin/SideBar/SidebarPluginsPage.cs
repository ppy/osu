using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.LLin.SideBar.PluginsPage;

namespace osu.Game.Screens.LLin.SideBar
{
    internal class SidebarPluginsPage : OsuScrollContainer, ISidebarContent
    {
        public string Title => "插件";
        public IconUsage Icon => FontAwesome.Solid.Boxes;

        [BackgroundDependencyLoader]
        private void load()
        {
            ScrollbarVisible = false;
            RelativeSizeAxes = Axes.Both;

            Add(new PluginsSection());
        }
    }
}
