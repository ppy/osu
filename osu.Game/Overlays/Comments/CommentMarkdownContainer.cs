// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Markdig.Syntax;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Overlays.Comments
{
    public class CommentMarkdownContainer : OsuMarkdownContainer
    {
        protected override MarkdownHeading CreateHeading(HeadingBlock headingBlock) => new CommentMarkdownHeading(headingBlock);

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
