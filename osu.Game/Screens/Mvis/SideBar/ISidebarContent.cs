using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Mvis.SideBar
{
    public interface ISidebarContent
    {
        [Obsolete("新版侧边栏已不再使用ResizeWidth")]
        public float ResizeWidth => 1f;

        public string Title { get; }

        public IconUsage Icon { get; }

        [Obsolete("新版侧边栏已不再使用ResizeHeight")]
        public float ResizeHeight => 1f;
    }
}
