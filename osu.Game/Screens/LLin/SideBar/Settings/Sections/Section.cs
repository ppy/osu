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
        public virtual int Columns => 3;

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

        protected virtual float PieceWidth => 150;

        protected Section()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.TopRight;
            Padding = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                title,
                FillFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Width = (PieceWidth * Columns) + (5 * (Columns - 1)),
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Top = 40 }
                }
            };
        }

        private Bindable<TabControlPosition> currentTabPosition;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            currentTabPosition = config.GetBindable<TabControlPosition>(MSetting.MvisTabControlPosition);
        }

        protected override void LoadComplete()
        {
            currentTabPosition.BindValueChanged(onTabPositionChanged, true);
            base.LoadComplete();
        }

        private void onTabPositionChanged(ValueChangedEvent<TabControlPosition> v)
        {
            switch (v.NewValue)
            {
                case TabControlPosition.Left:
                    title.Anchor = title.Origin = Anchor.TopLeft;
                    Anchor = Origin = Anchor.TopLeft;
                    break;

                case TabControlPosition.Right:
                    title.Anchor = title.Origin = Anchor.TopRight;
                    Anchor = Origin = Anchor.TopRight;
                    break;

                case TabControlPosition.Top:
                    title.Anchor = title.Origin = Anchor.TopCentre;
                    Anchor = Origin = Anchor.TopCentre;
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
