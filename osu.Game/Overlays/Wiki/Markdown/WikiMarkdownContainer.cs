// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using osu.Framework.Graphics.Containers;
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

        protected virtual FillFlowContainer CreateNotice(YamlFrontMatterBlock yamlFrontMatterBlock) => new WikiNoticeContainer(yamlFrontMatterBlock);
    }
}
