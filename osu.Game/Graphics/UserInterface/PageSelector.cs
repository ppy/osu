// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class PageSelector : CompositeDrawable
    {
        private BindableInt currentPage = new BindableInt(1);

        private readonly int maxPages;
        private readonly FillFlowContainer pillsFlow;

        public PageSelector(int maxPages)
        {
            this.maxPages = maxPages;

            AutoSizeAxes = Axes.Both;
            InternalChild = pillsFlow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentPage.BindValueChanged(page => redraw(page.NewValue), true);
        }

        private void redraw(int newPage)
        {
            pillsFlow.Clear();

            for (int i = 0; i < maxPages; i++)
            {
                addPagePill(i);
            }
        }

        private void addPagePill(int page)
        {
            var pill = new Pill(page);

            if (page != currentPage.Value)
                pill.Action = () => currentPage.Value = page;

            pillsFlow.Add(pill);
        }

        private class Pill : OsuClickableContainer
        {
            private const int height = 20;

            private readonly Box background;

            public Pill(int page)
            {
                AutoSizeAxes = Axes.X;
                Height = height;
                Child = new CircularContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = page.ToString(),
                            Margin = new MarginPadding { Horizontal = 8 }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Seafoam;
            }
        }
    }
}
