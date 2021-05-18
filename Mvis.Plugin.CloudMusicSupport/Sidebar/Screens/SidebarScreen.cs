using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public abstract class SidebarScreen : Screen
    {
        public virtual Drawable[] Entries => new Drawable[] { };
    }
}
