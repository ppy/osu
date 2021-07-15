// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
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
                new BlockMarkdownImage(linkInline),
                parentTextComponent.CreateSpriteText().With(t =>
                {
                    t.Text = linkInline.Title;
                    t.Anchor = Anchor.TopCentre;
                    t.Origin = Anchor.TopCentre;
                }),
            };
        }

        private class BlockMarkdownImage : WikiMarkdownImage
        {
            public BlockMarkdownImage(LinkInline linkInline)
                : base(linkInline)
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;
            }

            protected override ImageContainer CreateImageContainer(string url) => new BlockImageContainer(url);

            private class BlockImageContainer : ImageContainer
            {
                public BlockImageContainer(string url)
                    : base(url)
                {
                    AutoSizeAxes = Axes.Y;
                    RelativeSizeAxes = Axes.X;
                }

                protected override Sprite CreateImageSprite() => new ImageSprite();

                private class ImageSprite : Sprite
                {
                    public ImageSprite()
                    {
                        Anchor = Anchor.TopCentre;
                        Origin = Anchor.TopCentre;
                    }

                    protected override void Update()
                    {
                        base.Update();

                        if (Width > Parent.DrawWidth)
                        {
                            float ratio = Height / Width;
                            Width = Parent.DrawWidth;
                            Height = ratio * Width;
                        }
                    }
                }
            }
        }
    }
}
