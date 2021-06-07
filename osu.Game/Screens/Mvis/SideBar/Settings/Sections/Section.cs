using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        private readonly OsuSpriteText title = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 30),
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Margin = new MarginPadding { Right = 10 }
        };

        private readonly List<FillFlowContainer> containers = new List<FillFlowContainer>();

        protected FillFlowContainer CreateFillFlowContainer()
        {
            var target = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Spacing = new Vector2(10),
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };

            containers.Add(target);
            target.Margin = new MarginPadding { Top = 40, Right = 10 + 160 * containers.IndexOf(target) };

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
