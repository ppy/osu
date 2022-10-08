using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin.SideBar.Tabs;
using osuTK;

namespace osu.Game.Screens.LLin.SideBar.Settings.Sections
{
    public abstract class Section : CompositeDrawable, ISidebarContent
    {
        public string Title
        {
            get => title.Text.ToString();
            set => title.Text = value;
        }

        public IconUsage Icon { get; set; }

        private readonly OsuSpriteText title = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 30)
        };

        protected Section()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Anchor = Origin = Anchor.TopRight;
            Padding = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                title,
                FillFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Top = 40 }
                }
            };
        }

        [Cached]
        private Bindable<TabControlPosition> currentTabPosition { get; set; } = new Bindable<TabControlPosition>();

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisTabControlPosition, currentTabPosition);
        }

        protected override void LoadComplete()
        {
            currentTabPosition.BindValueChanged(OnTabPositionChanged, true);

            base.LoadComplete();
        }

        protected void FadeoutThen(double fadeOutDuration, Action action)
        {
            this.FadeTo(0.01f, fadeOutDuration, Easing.OutQuint)
                .Then()
                .Schedule(action.Invoke)
                .Then()
                .FadeIn(200, Easing.OutQuint);
        }

        protected virtual void OnTabPositionChanged(ValueChangedEvent<TabControlPosition> v)
        {
            switch (v.NewValue)
            {
                case TabControlPosition.Left:
                    title.Anchor = title.Origin = Anchor.TopLeft;
                    Anchor = Origin = Anchor.TopLeft;
                    FillFlow.Anchor = FillFlow.Origin = Anchor.TopLeft;
                    break;

                case TabControlPosition.Right:
                    title.Anchor = title.Origin = Anchor.TopRight;
                    Anchor = Origin = Anchor.TopRight;
                    FillFlow.Anchor = FillFlow.Origin = Anchor.TopRight;
                    break;

                case TabControlPosition.Top:
                    title.Anchor = title.Origin = Anchor.TopCentre;
                    Anchor = Origin = Anchor.TopCentre;
                    FillFlow.Anchor = FillFlow.Origin = Anchor.TopCentre;
                    break;
            }
        }

        protected readonly FillFlowContainer FillFlow;

        protected void AddRange(Drawable[] drawables)
        {
            foreach (var drawable in drawables)
            {
                Add(drawable);
            }
        }

        protected void Add(Drawable drawable) => FillFlow.Add(drawable);
    }
}
