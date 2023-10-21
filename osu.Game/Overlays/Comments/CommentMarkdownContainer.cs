// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Containers.Markdown;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public partial class CommentMarkdownContainer : OsuMarkdownContainer
    {
        protected override OsuMarkdownContainerOptions Options => new OsuMarkdownContainerOptions
        {
            Autolinks = true
        };

        protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock) => new CommentMarkdownHeading(headingBlock);

        public override MarkdownTextFlowContainer CreateTextFlow() => new CommentMarkdownTextFlowContainer();

        private partial class CommentMarkdownHeading : OsuMarkdownHeading
        {
            public CommentMarkdownHeading(HeadingBlock headingBlock)
                : base(headingBlock)
            {
            }

            protected override float GetFontSizeByLevel(int level)
            {
                float defaultFontSize = base.GetFontSizeByLevel(6);

                switch (level)
                {
                    case 1:
                        return 1.2f * defaultFontSize;

                    case 2:
                        return 1.1f * defaultFontSize;

                    default:
                        return defaultFontSize;
                }
            }
        }

        private partial class CommentMarkdownTextFlowContainer : MarkdownTextFlowContainer
        {
            protected override void AddImage(LinkInline linkInline) => AddDrawable(new CommentMarkdownImage(linkInline.Url));

            private partial class CommentMarkdownImage : MarkdownImage
            {
                public CommentMarkdownImage(string url)
                    : base(url)
                {
                }

                private DelayedLoadWrapper wrapper;

                protected override Drawable CreateContent(string url) => wrapper = new DelayedLoadWrapper(CreateImageContainer(url));

                protected override ImageContainer CreateImageContainer(string url)
                {
                    var container = new CommentImageContainer(url);
                    container.OnLoadComplete += d =>
                    {
                        // The size of DelayedLoadWrapper depends on AutoSizeAxes of it's content.
                        // But since it's set to None, we need to specify the size here manually.
                        wrapper.Size = container.Size;
                        d.FadeInFromZero(300, Easing.OutQuint);
                    };
                    return container;
                }

                private partial class CommentImageContainer : ImageContainer
                {
                    // https://github.com/ppy/osu-web/blob/3bd0f406dc78d60b356d955cd4201f8c3e1cca09/resources/css/bem/osu-md.less#L36
                    // Web version defines max height in em units (6 em), which assuming default pixel size as 14 results in 84 px,
                    // which also seems to match my observations upon expecting the web element.
                    private const float max_height = 84f;

                    public CommentImageContainer(string url)
                        : base(url)
                    {
                        AutoSizeAxes = Axes.None;
                    }

                    protected override Sprite CreateImageSprite() => new Sprite
                    {
                        RelativeSizeAxes = Axes.Both
                    };

                    protected override Texture GetImageTexture(TextureStore textures, string url)
                    {
                        Texture t = base.GetImageTexture(textures, url);

                        if (t != null)
                            Size = t.Height > max_height ? new Vector2(max_height / t.Height * t.Width, max_height) : t.Size;

                        return t;
                    }
                }
            }
        }
    }
}
