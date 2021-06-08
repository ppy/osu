using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

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
            Width = 45;
            RelativeSizeAxes = Axes.Y;

            Margin = new MarginPadding { Right = 5 };

            InternalChildren = new Drawable[]
            {
                new OsuScrollContainer(Direction.Vertical)
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollContent =
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight
                    },
                    Child = Tabs = new FillFlowContainer<HeaderTabItem>
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Margin = new MarginPadding { Vertical = 25 },
                        Spacing = new Vector2(5)
                    },
                    ScrollbarVisible = false
                }
            };
        }
    }
}
