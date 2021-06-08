using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
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
            Width = 50;
            RelativeSizeAxes = Axes.Y;

            Padding = new MarginPadding { Right = 5 };

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            InternalChild = new OsuScrollContainer(Direction.Vertical)
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
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Vertical = 25 },
                    Spacing = new Vector2(5)
                },
                ScrollbarVisible = false
            };
        }

        protected override void LoadComplete()
        {
            Hide();
            base.LoadComplete();
        }

        public bool SidebarActive
        {
            get => sidebarActive;
            set
            {
                //如果侧边栏关闭，并且光标不在tabHeader上，隐藏
                if (!IsHovered && value == false) Hide();

                sidebarActive = value;
            }
        }

        private bool sidebarActive;

        public Bindable<bool> IsVisible = new Bindable<bool>();

        public override void Show()
        {
            IsVisible.Value = true;
            this.MoveToX(0, 300, Easing.OutQuint);
            InternalChild.FadeIn(250, Easing.OutQuint);
        }

        public override void Hide()
        {
            if (IsHovered || SidebarActive) return;

            IsVisible.Value = false;

            this.MoveToX(5, 300, Easing.OutQuint);
            InternalChild.FadeOut(250, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            Show();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            InternalChild.FadeTo(0.99f, 500).OnComplete(_ => Hide());
            base.OnHoverLost(e);
        }
    }
}
