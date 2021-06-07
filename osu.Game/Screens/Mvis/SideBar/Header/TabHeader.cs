using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Mvis.SideBar.Header
{
    public class TabHeader : CompositeDrawable
    {
        public FillFlowContainer<HeaderTabItem> Tabs;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        public TabHeader()
        {
            Name = "Header";
            Height = 50;
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollContent =
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight
                    },
                    Child = Tabs = new FillFlowContainer<HeaderTabItem>
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Horizontal = 25 }
                    },
                    ScrollbarVisible = false
                }
            };
        }
    }
}
