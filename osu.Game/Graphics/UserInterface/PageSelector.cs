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
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class PageSelector : CompositeDrawable
    {
        public readonly BindableInt CurrentPage = new BindableInt(1);

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

            if (CurrentPage.Value == 1)
                addPreviousPageButton();
            else
                addPreviousPageButton(() => CurrentPage.Value -= 1);

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

            if (CurrentPage.Value == maxPages)
                addNextPageButton();
            else
                addNextPageButton(() => CurrentPage.Value += 1);
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

        private void addPreviousPageButton(Action action = null)
        {
            pillsFlow.Add(new PreviousPageButton(action));
        }

        private void addNextPageButton(Action action = null)
        {
            pillsFlow.Add(new NextPageButton(action));
        }

        private abstract class DrawablePage : CompositeDrawable
        {
            private const int height = 20;
            private const int margin = 8;

            protected readonly string Text;
            protected readonly Drawable Content;

            protected DrawablePage(string text)
            {
                Text = text;

                AutoSizeAxes = Axes.X;
                Height = height;

                var background = CreateBackground();

                if (background != null)
                    AddInternal(background);

                Content = CreateContent();
                Content.Margin = new MarginPadding { Horizontal = margin };

                AddInternal(Content);
            }

            protected abstract Drawable CreateContent();

            protected virtual Drawable CreateBackground() => null;
        }

        private abstract class ActivatedDrawablePage : DrawablePage
        {
            protected readonly Action Action;

            public ActivatedDrawablePage(string text, Action action = null)
                : base(text)
            {
                Action = action;
            }

            protected override bool OnClick(ClickEvent e)
            {
                Action?.Invoke();
                return base.OnClick(e);
            }
        }

        private class Page : ActivatedDrawablePage
        {
            private OsuColour colours;

            public Page(string text, Action action)
                : base(text, action)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;
                Content.Colour = colours.Seafoam;
            }

            protected override bool OnHover(HoverEvent e)
            {
                Content.Colour = colours.Seafoam.Lighten(30f);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Content.Colour = colours.Seafoam;
                base.OnHoverLost(e);
            }

            protected override Drawable CreateContent() => new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = Text
            };
        }

        private class SelectedPage : DrawablePage
        {
            private Box background;

            public SelectedPage(string text)
                : base(text)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Content.Colour = colours.GreySeafoam;
                background.Colour = colours.Seafoam;
            }

            protected override Drawable CreateContent() => new SpriteText
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
            public Placeholder()
                : base("...")
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Content.Colour = colours.Seafoam;
            }

            protected override Drawable CreateContent() => new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = Text
            };
        }

        private class PreviousPageButton : ActivatedDrawablePage
        {
            private OsuColour colours;
            private Box background;

            public PreviousPageButton(Action action)
                : base("prev", action)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;
                Content.Colour = colours.Seafoam;
                background.Colour = colours.GreySeafoam;

                if (Action == null)
                {
                    Content.FadeColour(colours.GrayA);
                    background.FadeColour(colours.GrayA);
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                Content.Colour = colours.Seafoam.Lighten(30f);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Content.Colour = colours.Seafoam;
                base.OnHoverLost(e);
            }

            protected override Drawable CreateContent() => new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(3),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.CaretLeft,
                        Size = new Vector2(10),
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = Text.ToUpper(),
                    }
                }
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

        private class NextPageButton : ActivatedDrawablePage
        {
            private OsuColour colours;
            private Box background;

            public NextPageButton(Action action)
                : base("next", action)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;
                Content.Colour = colours.Seafoam;
                background.Colour = colours.GreySeafoam;

                if (Action == null)
                {
                    Content.FadeColour(colours.GrayA);
                    background.FadeColour(colours.GrayA);
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                Content.Colour = colours.Seafoam.Lighten(30f);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Content.Colour = colours.Seafoam;
                base.OnHoverLost(e);
            }

            protected override Drawable CreateContent() => new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(3),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = Text.ToUpper(),
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.CaretRight,
                        Size = new Vector2(10),
                    },
                }
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
