using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SidebarSettingsScrollContainer : OsuScrollContainer, ISidebarContent
    {
        public float ResizeWidth => 0.3f;
        public string Title => "播放器设置";
    }
}
