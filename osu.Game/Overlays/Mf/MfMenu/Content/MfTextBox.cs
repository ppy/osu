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
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            Spacing = new Vector2(0, 2);
            Padding = new MarginPadding(25);
        }
    }
}