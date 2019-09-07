// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;
using System;
using osuTK;
using osu.Game.Graphics.Containers;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Graphics.UserInterface
{
    public class PageSelector : CompositeDrawable
    {
        public readonly BindableInt CurrentPage = new BindableInt(1);

        private readonly int maxPages;
        private readonly FillFlowContainer itemsFlow;

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
                    itemsFlow = new FillFlowContainer
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

            CurrentPage.BindValueChanged(page => redraw(page.NewValue), true);
        }

        private void redraw(int newPage)
        {
            previousPageButton.Enabled.Value = newPage != 1;
            nextPageButton.Enabled.Value = newPage != maxPages;

            itemsFlow.Clear();

            if (newPage > 3)
                addDrawablePage(1);

            if (newPage > 4)
                addPlaceholder();

            for (int i = Math.Max(newPage - 2, 1); i <= Math.Min(newPage + 2, maxPages); i++)
            {
                if (i == newPage)
                    addDrawableCurrentPage();
                else
                    addDrawablePage(i);
            }

            if (newPage + 2 < maxPages - 1)
                addPlaceholder();

            if (newPage + 2 < maxPages)
                addDrawablePage(maxPages);
        }

        private void addDrawablePage(int page) => itemsFlow.Add(new DrawablePage(page.ToString())
        {
            Action = () => CurrentPage.Value = page,
        });

        private void addPlaceholder() => itemsFlow.Add(new Placeholder());

        private void addDrawableCurrentPage() => itemsFlow.Add(new SelectedPage(CurrentPage.Value.ToString()));

        private abstract class PageItem : OsuHoverContainer
        {
            private const int margin = 8;
            private const int height = 20;

            protected PageItem(string text)
            {
                AutoSizeAxes = Axes.X;
                Height = height;

                var contentContainer = new CircularContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Masking = true,
                };

                var background = CreateBackground();
                if (background != null)
                    contentContainer.Add(background);

                var drawableText = CreateText(text);
                if (drawableText != null)
                {
                    drawableText.Margin = new MarginPadding { Horizontal = margin };
                    contentContainer.Add(drawableText);
                }

                Add(contentContainer);
            }

            protected abstract Drawable CreateText(string text);

            protected abstract Drawable CreateBackground();
        }

        private class DrawablePage : PageItem
        {
            protected SpriteText SpriteText;

            protected override IEnumerable<Drawable> EffectTargets => new[] { SpriteText };

            public DrawablePage(string text)
                : base(text)
            {
            }

            protected override Drawable CreateBackground() => null;

            protected override Drawable CreateText(string text) => SpriteText = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = text,
            };

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.Seafoam;
                HoverColour = colours.Seafoam.Lighten(30f);
            }
        }

        private class SelectedPage : DrawablePage
        {
            private Box background;

            protected override IEnumerable<Drawable> EffectTargets => null;

            public SelectedPage(string text)
                : base(text)
            {
            }

            protected override Drawable CreateBackground() => background = new Box
            {
                RelativeSizeAxes = Axes.Both,
            };

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Seafoam;
                SpriteText.Colour = colours.GreySeafoamDark;
            }
        }

        private class Placeholder : DrawablePage
        {
            public Placeholder()
                : base("...")
            {
            }
        }

        private class Button : PageItem
        {
            private Box background;
            private FillFlowContainer textContainer;
            private SpriteIcon icon;

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            public Button(bool rightAligned, string text)
                : base(text)
            {
                var alignment = rightAligned ? Anchor.x0 : Anchor.x2;

                textContainer.ForEach(drawable =>
                {
                    drawable.Anchor = Anchor.y1 | alignment;
                    drawable.Origin = Anchor.y1 | alignment;
                });

                icon.Icon = alignment == Anchor.x2 ? FontAwesome.Solid.ChevronLeft : FontAwesome.Solid.ChevronRight;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.GreySeafoamDark;
                HoverColour = colours.GrayA;
            }

            protected override Drawable CreateBackground() => background = new Box
            {
                RelativeSizeAxes = Axes.Both,
            };

            protected override Drawable CreateText(string text) => textContainer = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Text = text.ToUpper(),
                    },
                    icon = new SpriteIcon
                    {
                        Size = new Vector2(10),
                    },
                }
            };
        }
    }
}
