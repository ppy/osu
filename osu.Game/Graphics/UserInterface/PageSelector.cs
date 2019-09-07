// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
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
        public readonly BindableInt CurrentPage = new BindableInt();

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

            CurrentPage.BindValueChanged(_ => redraw(), true);
        }

        private void redraw()
        {
            pillsFlow.Clear();

            if (CurrentPage.Value > 3)
                addDrawablePage(1);

            if (CurrentPage.Value > 4)
                addPlaceholder();

            for (int i = Math.Max(CurrentPage.Value - 2, 1); i <= Math.Min(CurrentPage.Value + 2, maxPages); i++)
            {
                if (i == CurrentPage.Value)
                    addCurrentPagePill();
                else
                    addDrawablePage(i);
            }

            if (CurrentPage.Value + 2 < maxPages - 1)
                addPlaceholder();

            if (CurrentPage.Value + 2 < maxPages)
                addDrawablePage(maxPages);
        }

        private void addDrawablePage(int page)
        {
            pillsFlow.Add(new Page(page.ToString(), () => CurrentPage.Value = page));
        }

        private void addPlaceholder()
        {
            pillsFlow.Add(new Placeholder());
        }

        private void addCurrentPagePill()
        {
            pillsFlow.Add(new SelectedPage(CurrentPage.Value.ToString()));
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

        private class SelectedPage : DrawablePage
        {
            private SpriteText text;

            private Box background;

            public SelectedPage(string text)
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

        private class Placeholder : DrawablePage
        {
            private SpriteText text;

            public Placeholder()
                : base("...")
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                text.Colour = colours.Seafoam;
            }

            protected override Drawable CreateContent() => text = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = Text
            };
        }
    }
}
