// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiMarkdownContainer : OsuMarkdownContainer
    {
        public string CurrentPath
        {
            set => Schedule(() => DocumentUrl += $"wiki/{value}");
        }

        protected override void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int level)
        {
            switch (markdownObject)
            {
                case YamlFrontMatterBlock yamlFrontMatterBlock:
                    container.Add(CreateNotice(yamlFrontMatterBlock));
                    break;

                default:
                    base.AddMarkdownComponent(markdownObject, container, level);
                    break;
            }
        }

        public override MarkdownTextFlowContainer CreateTextFlow() => new WikiMarkdownTextFlowContainer();

        protected override MarkdownParagraph CreateParagraph(ParagraphBlock paragraphBlock, int level) => new WikiMarkdownParagraph(paragraphBlock);

        protected virtual FillFlowContainer CreateNotice(YamlFrontMatterBlock yamlFrontMatterBlock) => new WikiNoticeContainer(yamlFrontMatterBlock);

        private class WikiMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            protected override void AddImage(LinkInline linkInline) => AddDrawable(new WikiMarkdownImage(linkInline));
        }
    }
}
