// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogMarkdownContainer : OsuMarkdownContainer
    {
        public override SpriteText CreateSpriteText() => new OsuSpriteText
        {
            Font = base.CreateSpriteText().Font.With(size: 12)
        };

        protected override void AddMarkdownComponent(IMarkdownObject markdownObject, FillFlowContainer container, int level)
        {
            switch (markdownObject)
            {
                case ParagraphBlock paragraphBlock:
                    if (paragraphBlock.Inline.FirstChild is HtmlInline firstChild &&
                        paragraphBlock.Inline.LastChild is HtmlInline lastChild &&
                        firstChild.Tag.Contains("video") && lastChild.Tag.Contains("video"))
                    {
                        // Don't render paragraph that only contains HTML video
                        return;
                    }

                    break;
            }

            base.AddMarkdownComponent(markdownObject, container, level);
        }
    }
}
