using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.LLin.SideBar
{
    public interface ISidebarContent
    {
        public string Title { get; }

        public IconUsage Icon { get; }
    }
}
