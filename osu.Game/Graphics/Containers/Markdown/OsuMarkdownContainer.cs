// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownContainer : MarkdownContainer
    {
        protected override void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int level)
        {
            switch (markdownObject)
            {
                case YamlFrontMatterBlock _:
                    // Don't parse YAML Frontmatter
                    break;

                case ListItemBlock listItemBlock:
                    var childContainer = CreateListItem(listItemBlock, level);
                    container.Add(childContainer);
                    foreach (var single in listItemBlock)
                        base.AddMarkdownComponent(single, childContainer.Content, level);
                    break;

                default:
                    base.AddMarkdownComponent(markdownObject, container, level);
                    break;
            }
        }

        public override MarkdownTextFlowContainer CreateTextFlow() => new OsuMarkdownTextFlowContainer();

        protected override MarkdownFencedCodeBlock CreateFencedCodeBlock(FencedCodeBlock fencedCodeBlock) => new OsuMarkdownFencedCodeBlock(fencedCodeBlock);

        protected override MarkdownSeparator CreateSeparator(ThematicBreakBlock thematicBlock) => new OsuMarkdownSeparator();

        protected override MarkdownQuoteBlock CreateQuoteBlock(QuoteBlock quoteBlock) => new OsuMarkdownQuoteBlock(quoteBlock);

        protected override MarkdownTable CreateTable(Table table) => new OsuMarkdownTable(table);

        protected override MarkdownList CreateList(ListBlock listBlock) => new MarkdownList
        {
            Padding = new MarginPadding(0)
        };

        protected virtual OsuMarkdownListItem CreateListItem(ListItemBlock listItemBlock, int level) => new OsuMarkdownListItem(level);

        protected override MarkdownPipeline CreateBuilder()
            => new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                                            .UseEmojiAndSmiley()
                                            .UseYamlFrontMatter()
                                            .UseAdvancedExtensions().Build();
    }
}
