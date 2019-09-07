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
using osu.Game.Graphics.Containers;
using System.Collections.Generic;

namespace osu.Game.Graphics.UserInterface
{
    public class PageSelector : CompositeDrawable
    {
        public readonly BindableInt CurrentPage = new BindableInt(1);

        private readonly int maxPages;
        private readonly FillFlowContainer pillsFlow;

        private readonly Button previousPageButton;
        private readonly Button nextPageButton;

        public PageSelector(int maxPages)
        {
            this.maxPages = maxPages;

            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    previousPageButton = new Button(false, "prev")
                    {
                        Action = () => CurrentPage.Value -= 1,
                    },
                    pillsFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                    },
                    nextPageButton = new Button(true, "next")
                    {
                        Action = () => CurrentPage.Value += 1
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentPage.BindValueChanged(_ => redraw(), true);
        }

        private void redraw()
        {
            previousPageButton.Enabled.Value = CurrentPage.Value != 1;
            nextPageButton.Enabled.Value = CurrentPage.Value != maxPages;

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

        private class Button : OsuHoverContainer
        {
            private const int height = 20;
            private const int margin = 8;

            private readonly Anchor alignment;
            private readonly Box background;

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            public Button(bool rightAligned, string text)
            {
                alignment = rightAligned ? Anchor.x0 : Anchor.x2;

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
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Horizontal = margin },
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Anchor = Anchor.y1 | alignment,
                                        Origin = Anchor.y1 | alignment,
                                        Text = text.ToUpper(),
                                    },
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.y1 | alignment,
                                        Origin = Anchor.y1 | alignment,
                                        Icon = alignment == Anchor.x2 ? FontAwesome.Solid.ChevronLeft : FontAwesome.Solid.ChevronRight,
                                        Size = new Vector2(10),
                                    },
                                }
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.GreySeafoamDark;
                HoverColour = colours.GrayA;
            }
        }
    }
}
