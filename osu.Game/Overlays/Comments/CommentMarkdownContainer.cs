// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Overlays.Comments
{
    public class CommentMarkdownContainer : OsuMarkdownContainer
    {
        public override MarkdownTextFlowContainer CreateTextFlow() => new CommentMarkdownTextFlowContainer();

        protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock) => new CommentMarkdownHeading(headingBlock);

        private class CommentMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            // Don't render image in comment for now
            protected override void AddImage(LinkInline linkInline) { }
        }

        private class CommentMarkdownHeading : OsuMarkdownHeading
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
    }
}
