using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfTextBox : LinkFlowContainer
    {
        public MfTextBox()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            TextAnchor = Anchor.TopLeft;
            Spacing = new Vector2(0, 2);
        }
    }
}