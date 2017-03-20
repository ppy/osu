using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A type of circle piece that will be scrolling the screen.
    /// <para>
    /// A scrolling circle piece must always have a centre-left origin due to how scroll position is calculated.
    /// </para>
    /// </summary>
    public class ScrollingCirclePiece : Container
    {
        public override Anchor Origin => Anchor.CentreLeft;
    }
}