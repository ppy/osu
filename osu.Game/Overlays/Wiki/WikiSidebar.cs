// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiSidebar : OverlaySidebar
    {
        private WikiTableOfContents tableOfContents;

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            Direction = FillDirection.Vertical,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = WikiStrings.ShowToc.ToUpper(),
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                    Margin = new MarginPadding { Bottom = 5 },
                },
                tableOfContents = new WikiTableOfContents(),
            },
        };

        public void AddEntry(HeadingBlock headingBlock, MarkdownHeading heading)
        {
            switch (headingBlock.Level)
            {
                case 2:
                case 3:
                    tableOfContents.AddEntry(getTitle(headingBlock.Inline), heading, headingBlock.Level == 3);
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

                    case LinkInline linkInline:
                        if (!linkInline.IsImage)
                            return getTitle(linkInline);

                        break;
                }
            }

            return string.Empty;
        }
    }
}
