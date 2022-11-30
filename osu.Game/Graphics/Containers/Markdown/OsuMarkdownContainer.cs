// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Markdig;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Tables;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownContainer : MarkdownContainer
    {
        /// <summary>
        /// Allows this markdown container to parse and link footnotes.
        /// </summary>
        /// <seealso cref="FootnoteExtension"/>
        protected virtual bool Footnotes => false;

        /// <summary>
        /// Allows this markdown container to make URL text clickable.
        /// </summary>
        /// <seealso cref="AutoLinkExtension"/>
        protected virtual bool Autolinks => false;

        /// <summary>
        /// Allows this markdown container to parse custom containers (used for flags and infoboxes).
        /// </summary>
        /// <seealso cref="CustomContainerExtension"/>
        protected virtual bool CustomContainers => false;

        public OsuMarkdownContainer()
        {
            LineSpacing = 21;
        }

        protected override void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int level)
        {
            switch (markdownObject)
            {
                case YamlFrontMatterBlock:
                    // Don't parse YAML Frontmatter
                    break;

                case ListItemBlock listItemBlock:
                    bool isOrdered = ((ListBlock)listItemBlock.Parent)?.IsOrdered == true;

                    OsuMarkdownListItem childContainer = CreateListItem(listItemBlock, level, isOrdered);

                    container.Add(childContainer);

                    foreach (var single in listItemBlock)
                        base.AddMarkdownComponent(single, childContainer.Content, level);
                    break;

                default:
                    base.AddMarkdownComponent(markdownObject, container, level);
                    break;
            }
        }

        public override SpriteText CreateSpriteText() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(Typeface.Inter, size: 14, weight: FontWeight.Regular),
        };

        public override MarkdownTextFlowContainer CreateTextFlow() => new OsuMarkdownTextFlowContainer();

        protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock) => new OsuMarkdownHeading(headingBlock);

        protected override MarkdownFencedCodeBlock CreateFencedCodeBlock(FencedCodeBlock fencedCodeBlock) => new OsuMarkdownFencedCodeBlock(fencedCodeBlock);

        protected override MarkdownSeparator CreateSeparator(ThematicBreakBlock thematicBlock) => new OsuMarkdownSeparator();

        protected override MarkdownQuoteBlock CreateQuoteBlock(QuoteBlock quoteBlock) => new OsuMarkdownQuoteBlock(quoteBlock);

        protected override MarkdownTable CreateTable(Table table) => new OsuMarkdownTable(table);

        protected override MarkdownList CreateList(ListBlock listBlock) => new MarkdownList
        {
            Padding = new MarginPadding(0)
        };

        protected virtual OsuMarkdownListItem CreateListItem(ListItemBlock listItemBlock, int level, bool isOrdered)
        {
            if (isOrdered)
                return new OsuMarkdownOrderedListItem(listItemBlock.Order);

            return new OsuMarkdownUnorderedListItem(level);
        }

        // reference: https://github.com/ppy/osu-web/blob/05488a96b25b5a09f2d97c54c06dd2bae59d1dc8/app/Libraries/Markdown/OsuMarkdown.php#L301
        protected override MarkdownPipeline CreateBuilder()
        {
            var pipeline = new MarkdownPipelineBuilder()
                           .UseAutoIdentifiers()
                           .UsePipeTables()
                           .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                           .UseYamlFrontMatter();

            if (Footnotes)
                pipeline = pipeline.UseFootnotes();

            if (Autolinks)
                pipeline = pipeline.UseAutoLinks();

            if (CustomContainers)
                pipeline.UseCustomContainers();

            return pipeline.Build();
        }
    }
}
