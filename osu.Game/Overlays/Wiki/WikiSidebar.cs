// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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

        public void AddToc(string title, MarkdownHeading heading, int level)
        {
            switch (level)
            {
                case 2:
                case 3:
                    tableOfContents.Add(new TocTitle(title, heading, level == 3));
                    break;
            }
        }

        private class TocTitle : OsuHoverContainer
        {
            [Resolved]
            private OverlayScrollContainer scrollContainer { get; set; }

            private readonly MarkdownHeading target;

            private readonly OsuTextFlowContainer textFlow;

            public TocTitle(string text, MarkdownHeading target, bool subtitle = false)
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
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
            }

            protected override bool OnClick(ClickEvent e)
            {
                scrollContainer.ScrollTo(target);
                return base.OnClick(e);
            }
        }
    }
}
