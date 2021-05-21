using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar.Header
{
    public class TabHeader : CompositeDrawable
    {
        private readonly Box highLightBox;
        private readonly Box bgBox;

        public FillFlowContainer<HeaderTabItem> Tabs;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        public TabHeader()
        {
            Name = "Header";
            Height = 40;
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
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
                        Spacing = new Vector2(10, 0),
                        Margin = new MarginPadding { Horizontal = 25 }
                    },
                    ScrollbarVisible = false
                },
                highLightBox = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                }
            };
        }

        protected override void LoadComplete()
        {
            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.Dark5;
                highLightBox.Colour = colourProvider.Highlight1;
            }, true);

            base.LoadComplete();
        }
    }
}
