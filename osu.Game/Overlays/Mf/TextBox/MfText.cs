using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Mf.TextBox
{
    public class MfLinkFlowContainer : LinkFlowContainer
    {
        public MfLinkFlowContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            TextAnchor = Anchor.TopLeft;
            Spacing = new Vector2(0, 2);
        }
    }
}
