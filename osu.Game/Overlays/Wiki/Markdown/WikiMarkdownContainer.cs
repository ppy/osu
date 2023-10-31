// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public partial class WikiMarkdownContainer : OsuMarkdownContainer
    {
        protected override OsuMarkdownContainerOptions Options => new OsuMarkdownContainerOptions
        {
            Footnotes = true,
            CustomContainers = true,
            BlockAttributes = true
        };

        public string CurrentPath
        {
            set => DocumentUrl = value;
        }

        protected override void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int level)
        {
            switch (markdownObject)
            {
                case CustomContainer:
                    // infoboxes are parsed into CustomContainer objects, but we don't have support for infoboxes yet.
                    // todo: add support for infobox.
                    break;

                case YamlFrontMatterBlock yamlFrontMatterBlock:
                    container.Add(new WikiNoticeContainer(yamlFrontMatterBlock));
                    break;

                case ParagraphBlock paragraphBlock:
                    // Check if paragraph only contains an image
                    if (paragraphBlock.Inline?.Count() == 1 && paragraphBlock.Inline.FirstChild is LinkInline { IsImage: true } linkInline)
                    {
                        container.Add(new WikiMarkdownImageBlock(linkInline));
                        return;
                    }

                    break;
            }

            base.AddMarkdownComponent(markdownObject, container, level);
        }

        public override MarkdownTextFlowContainer CreateTextFlow() => new WikiMarkdownTextFlowContainer();

        private partial class WikiMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            protected override void AddImage(LinkInline linkInline) => AddDrawable(new WikiMarkdownImage(linkInline));
        }
    }
}
