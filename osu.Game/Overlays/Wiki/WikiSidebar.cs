// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Wiki
{
    public class WikiSidebar : OverlaySidebar
    {
        private FillFlowContainer tableOfContents;

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            Direction = FillDirection.Vertical,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "CONTENTS",
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                },
                tableOfContents = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            },
        };

        public void AddEntry(HeadingBlock headingBlock, MarkdownHeading heading)
        {
            switch (headingBlock.Level)
            {
                case 2:
                case 3:
                    string title = getTitle(headingBlock.Inline);
                    tableOfContents.Add(new TableOfContentsEntry(title, heading, headingBlock.Level == 3));
                    break;
            }
        }

        private string getTitle(ContainerInline containerInline)
        {
            foreach (var inline in containerInline)
            {
                switch (inline)
                {
                    case LiteralInline literalInline:
                        return literalInline.Content.ToString();

                    case LinkInline { IsImage: false } linkInline:
                        return getTitle(linkInline);
                }
            }

            return string.Empty;
        }

        private class TableOfContentsEntry : OsuHoverContainer
        {
            [Resolved]
            private OverlayScrollContainer scrollContainer { get; set; }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            private readonly MarkdownHeading target;

            private readonly OsuTextFlowContainer textFlow;

            public TableOfContentsEntry(string text, MarkdownHeading target, bool subtitle = false)
            {
                this.target = target;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Child = textFlow = new OsuTextFlowContainer(t =>
                {
                    t.Font = OsuFont.GetFont(size: subtitle ? 12 : 15);
                }).With(f =>
                {
                    f.AddText(text);
                    f.RelativeSizeAxes = Axes.X;
                    f.AutoSizeAxes = Axes.Y;
                });
                Margin = new MarginPadding { Top = subtitle ? 5 : 10 };
                Padding = new MarginPadding { Left = subtitle ? 10 : 0 };
            }

            protected override IEnumerable<Drawable> EffectTargets => new Drawable[] { textFlow };

            [BackgroundDependencyLoader]
            private void load()
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
            }

            protected override bool OnClick(ClickEvent e)
            {
                IdleColour = colourProvider.Light1;
                scrollContainer.ScrollTo(target);
                return base.OnClick(e);
            }
        }
    }
}
