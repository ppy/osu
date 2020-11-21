using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Modules;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar.Header
{
    public class TabHeader : Container
    {
        private readonly Box highLightBox;
        private readonly Box bgBox;

        public FillFlowContainer<HeaderTabItem> Tabs;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        public TabHeader()
        {
            OsuScrollContainer scroll;
            Name = "Header";
            Height = 40;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                scroll = new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = Tabs = new FillFlowContainer<HeaderTabItem>
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Margin = new MarginPadding { Right = 50 }
                    }
                },
                highLightBox = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                }
            };

            scroll.ScrollContent.Anchor = scroll.ScrollContent.Origin = Anchor.CentreRight;
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.Background5;
                highLightBox.Colour = colourProvider.Highlight1;
            }, true);

            base.LoadComplete();
        }
    }
}
