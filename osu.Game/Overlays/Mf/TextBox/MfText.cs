using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfText : LinkFlowContainer
    {
        public MfText()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            TextAnchor = Anchor.TopLeft;
            Spacing = new Vector2(0, 2);
        }
    }
}