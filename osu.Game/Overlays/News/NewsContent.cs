using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.News
{
    public abstract class NewsContent : FillFlowContainer
    {
        public NewsContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Bottom = 100 };
        }
    }
}
