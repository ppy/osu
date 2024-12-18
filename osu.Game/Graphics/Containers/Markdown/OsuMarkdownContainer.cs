// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Tables;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Containers.Markdown.Footnotes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers.Markdown.Footnotes;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown
{
    [Cached]
    public partial class OsuMarkdownContainer : MarkdownContainer
    {
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
                    // `ListBlock.Parent` is annotated as null-returning in xmldoc.
                    // Unfortunately code analysis sees that the type doesn't have NRT enabled and complains.
                    // This is fixed upstream in 0.24.0 (https://github.com/xoofx/markdig/commit/6684c8257cbbcba2d34457020876be289d3cd8b9),
                    // but markdig is a transitive dependency from framework, wherein we are locked to 0.23.0
                    // (https://github.com/ppy/osu-framework/blob/9746d7d06f48910c05a24687a25f435f30d12f8b/osu.Framework/osu.Framework.csproj#L52C1-L54)
                    // Therefore...
                    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
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

        public override OsuMarkdownTextFlowContainer CreateTextFlow() => new OsuMarkdownTextFlowContainer();

        protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock) => new OsuMarkdownHeading(headingBlock);

        protected override MarkdownCodeBlock CreateCodeBlock(CodeBlock codeBlock) => new OsuMarkdownCodeBlock(codeBlock);

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

        protected override MarkdownFootnoteGroup CreateFootnoteGroup(FootnoteGroup footnoteGroup) => base.CreateFootnoteGroup(footnoteGroup).With(g => g.Spacing = new Vector2(5));

        protected override MarkdownFootnote CreateFootnote(Footnote footnote) => new OsuMarkdownFootnote(footnote);

        protected sealed override MarkdownPipeline CreateBuilder()
            => Options.BuildPipeline();

        /// <summary>
        /// Creates a <see cref="OsuMarkdownContainerOptions"/> instance which is used to determine
        /// which CommonMark/Markdig extensions should be enabled for this <see cref="OsuMarkdownContainer"/>.
        /// </summary>
        protected virtual OsuMarkdownContainerOptions Options => new OsuMarkdownContainerOptions();
    }
}
