// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osuTK;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiMarkdownParagraph : MarkdownParagraph
    {
        private readonly ParagraphBlock paragraphBlock;

        public WikiMarkdownParagraph(ParagraphBlock paragraphBlock)
            : base(paragraphBlock)
        {
            this.paragraphBlock = paragraphBlock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            MarkdownTextFlowContainer textFlow;
            InternalChild = textFlow = CreateTextFlow();
            textFlow.AddInlineText(paragraphBlock.Inline);

            // Check if paragraph only contains an image.
            if (paragraphBlock.Inline.Count() == 1 && paragraphBlock.Inline.FirstChild is LinkInline { IsImage: true } linkInline)
            {
                textFlow.TextAnchor = Anchor.TopCentre;
                textFlow.Spacing = new Vector2(0, 5);
                textFlow.AddText($"\n{linkInline.Title}");
            }
        }
    }
}
