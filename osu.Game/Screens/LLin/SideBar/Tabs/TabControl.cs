using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.LLin.SideBar.Tabs
{
    internal class TabControl : CompositeDrawable
    {
        public FillFlowContainer<TabControlItem> Tabs;

        public float GetRightUnavaliableSpace() => anchorTarget?.Value == TabControlPosition.Right
            ? (Width + 5)
            : 0;

        public float GetLeftUnavaliableSpace() => anchorTarget?.Value == TabControlPosition.Left
            ? (Width + 5)
            : 0;

        public float GetTopUnavaliableSpace() => anchorTarget?.Value == TabControlPosition.Top
            ? (Height + 5)
            : 0;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private Bindable<TabControlPosition> anchorTarget;

        public TabControl()
        {
            Name = "Header";
            Width = 50;
            RelativeSizeAxes = Axes.Y;

            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;

            Tabs = new FillFlowContainer<TabControlItem>
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5)
            };

            InternalChild = scrollContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                Children = new Drawable[]
                {
                    verticalScroll = new OsuScrollContainer(Direction.Vertical)
                    {
                        RelativeSizeAxes = Axes.Both,
                        ScrollContent =
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight
                        },
                        ScrollbarVisible = false
                    },
                    horizonalScroll = new OsuScrollContainer(Direction.Horizontal)
                    {
                        RelativeSizeAxes = Axes.Both,
                        ScrollContent =
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                        ScrollbarVisible = false
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            anchorTarget = config.GetBindable<TabControlPosition>(MSetting.MvisTabControlPosition);

            anchorTarget.BindValueChanged(onTabControlPosChanged, true);
        }

        private void onTabControlPosChanged(ValueChangedEvent<TabControlPosition> v)
        {
            toggleMode(v.NewValue);

            switch (v.NewValue)
            {
                case TabControlPosition.Right:
                    Anchor = Anchor.CentreRight;
                    Origin = Anchor.CentreRight;
                    Tabs.Padding = new MarginPadding { Right = 5 };
                    break;

                case TabControlPosition.Left:
                    Anchor = Anchor.CentreLeft;
                    Origin = Anchor.CentreLeft;
                    Tabs.Padding = new MarginPadding { Left = 5 };
                    break;

                case TabControlPosition.Top:
                    Anchor = Anchor.TopCentre;
                    Origin = Anchor.TopCentre;
                    Tabs.Padding = new MarginPadding { Top = 5 };
                    break;
            }
        }

        private Vector2 targetHidePos = new Vector2(0);

        private void toggleMode(TabControlPosition newPos)
        {
            if (newPos == TabControlPosition.Left || newPos == TabControlPosition.Right)
            {
                RelativeSizeAxes = Axes.Y;
                Height = 1;
                Width = 50;

                targetHidePos = new Vector2(newPos == TabControlPosition.Right ? 5 : -5, 0);

                Tabs.Margin = new MarginPadding { Vertical = 25 };
                Tabs.Direction = FillDirection.Vertical;

                Tabs.Anchor = Tabs.Origin = Anchor.CentreRight;

                if (!verticalScroll.Contains(Tabs))
                {
                    horizonalScroll.Remove(Tabs);
                    verticalScroll.Add(Tabs);
                }

                verticalScroll.FadeIn();
                horizonalScroll.FadeOut();
            }
            else if (newPos == TabControlPosition.Top)
            {
                RelativeSizeAxes = Axes.X;
                Height = 50;
                Width = 1;

                targetHidePos = new Vector2(0, -5);

                Tabs.Margin = new MarginPadding { Horizontal = 25 };
                Tabs.Direction = FillDirection.Horizontal;

                Tabs.Anchor = Tabs.Origin = Anchor.TopCentre;

                if (!horizonalScroll.Contains(Tabs))
                {
                    verticalScroll.Remove(Tabs);
                    horizonalScroll.Add(Tabs);
                }

                verticalScroll.FadeOut();
                horizonalScroll.FadeIn();
            }

            if (!IsVisible.Value) this.MoveTo(targetHidePos);
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
        private readonly OsuScrollContainer verticalScroll;
        private readonly OsuScrollContainer horizonalScroll;
        private readonly Container scrollContainer;

        public override void Show()
        {
            IsVisible.Value = true;
            this.MoveTo(new Vector2(0), 300, Easing.OutQuint);
            scrollContainer.FadeIn(250, Easing.OutQuint);
        }

        public override void Hide()
        {
            if (IsHovered || SidebarActive) return;

            IsVisible.Value = false;

            this.MoveTo(targetHidePos, 300, Easing.OutQuint);
            scrollContainer.FadeOut(250, Easing.OutQuint);
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
