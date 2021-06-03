// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osuTK;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiMarkdownImageBlock : FillFlowContainer
    {
        [Resolved]
        private IMarkdownTextComponent parentTextComponent { get; set; }

        private readonly LinkInline linkInline;

        public WikiMarkdownImageBlock(LinkInline linkInline)
        {
            this.linkInline = linkInline;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, 3);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new WikiMarkdownImage(linkInline)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                parentTextComponent.CreateSpriteText().With(t =>
                {
                    t.Text = linkInline.Title;
                    t.Anchor = Anchor.TopCentre;
                    t.Origin = Anchor.TopCentre;
                }),
            };
        }
    }
}
