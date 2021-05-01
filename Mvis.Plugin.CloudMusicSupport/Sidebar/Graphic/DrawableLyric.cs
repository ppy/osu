using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public abstract class DrawableLyric : CompositeDrawable
    {
        public Lyric Value { get; protected set; }
    }
}
