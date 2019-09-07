// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Extensions.Color4Extensions;
using System;

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

            for (int i = 1; i <= maxPages; i++)
            {
                if (i == currentPage.Value)
                    addCurrentPagePill();
                else
                    addPagePill(i);
            }
        }

        private void addPagePill(int page)
        {
            pillsFlow.Add(new Page(page.ToString(), () => currentPage.Value = page));
        }

        private void addCurrentPagePill()
        {
            pillsFlow.Add(new CurrentPage(currentPage.Value.ToString()));
        }

        private abstract class DrawablePage : CompositeDrawable
        {
            private const int height = 20;
            private const int margin = 8;

            protected readonly string Text;

            protected DrawablePage(string text)
            {
                Text = text;

                AutoSizeAxes = Axes.X;
                Height = height;

                var background = CreateBackground();

                if (background != null)
                    AddInternal(background);

                var content = CreateContent();
                content.Margin = new MarginPadding { Horizontal = margin };

                AddInternal(content);
            }

            protected abstract Drawable CreateContent();

            protected virtual Drawable CreateBackground() => null;
        }

        private abstract class ActivatedDrawablePage : DrawablePage
        {
            private readonly Action action;

            public ActivatedDrawablePage(string text, Action action)
                : base(text)
            {
                this.action = action;
            }

            protected override bool OnClick(ClickEvent e)
            {
                action?.Invoke();
                return base.OnClick(e);
            }
        }

        private class Page : ActivatedDrawablePage
        {
            private SpriteText text;

            private OsuColour colours;

            public Page(string text, Action action)
                : base(text, action)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;
                text.Colour = colours.Seafoam;
            }

            protected override bool OnHover(HoverEvent e)
            {
                text.Colour = colours.Seafoam.Lighten(30f);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                text.Colour = colours.Seafoam;
                base.OnHoverLost(e);
            }

            protected override Drawable CreateContent() => text = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = Text
            };
        }

        private class CurrentPage : DrawablePage
        {
            private SpriteText text;

            private Box background;

            public CurrentPage(string text)
                : base(text)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                text.Colour = colours.GreySeafoam;
                background.Colour = colours.Seafoam;
            }

            protected override Drawable CreateContent() => text = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = Text
            };

            protected override Drawable CreateBackground() => new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }
    }
}
