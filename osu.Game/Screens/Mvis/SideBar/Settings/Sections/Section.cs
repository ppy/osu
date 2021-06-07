using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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

        private readonly List<FillFlowContainer> containers = new List<FillFlowContainer>();

        protected FillFlowContainer CreateFillFlowContainer()
        {
            var target = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };

            containers.Add(target);
            target.Margin = new MarginPadding { Top = 40, Right = 160 * containers.IndexOf(target) + 5 * containers.IndexOf(target) };

            return target;
        }

        protected Section()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Origin = Anchor.TopRight;
            InternalChildren = new Drawable[]
            {
                title
            };
            Padding = new MarginPadding(10);

            for (int i = 0; i <= Columns; i++)
            {
                AddInternal(CreateFillFlowContainer());
            }
        }

        private int index;

        protected void AddRange(Drawable[] drawables)
        {
            foreach (var drawable in drawables)
            {
                containers[index % Columns].Add(drawable);
                index++;
            }
        }
    }
}
