using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.SideBar.Tabs;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Sections
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
            Font = OsuFont.GetFont(size: 30),
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight
        };

        protected readonly List<FillFlowContainer> Containers = new List<FillFlowContainer>();

        protected virtual float PieceWidth => 150;

        protected FillFlowContainer CreateFillFlowContainer()
        {
            var target = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(5),
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };

            Containers.Add(target);
            target.Margin = new MarginPadding { Top = 40, Right = (PieceWidth * Containers.IndexOf(target)) + (5 * Containers.IndexOf(target)) };

            return target;
        }

        protected Section()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.TopRight;
            InternalChild = title;
            Padding = new MarginPadding(10);

            for (int i = 0; i <= Columns; i++)
                AddInternal(CreateFillFlowContainer());
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

        private int index;

        protected void AddRange(Drawable[] drawables)
        {
            foreach (var drawable in drawables)
            {
                Add(drawable);
            }
        }

        protected void Add(Drawable drawable)
        {
            Containers[index % Columns].Add(drawable);
            index++;
        }
    }
}
