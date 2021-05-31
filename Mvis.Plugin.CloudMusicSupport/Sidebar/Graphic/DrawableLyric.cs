using System;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Graphics.Pooling;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public abstract class DrawableLyric : PoolableDrawable, IComparable<DrawableLyric>
    {
        public Lyric Value { get; set; }
        public float CurrentY;
        public int CompareTo(DrawableLyric other) => CurrentY.CompareTo(other.CurrentY);
        public abstract int FinalHeight();
    }
}
