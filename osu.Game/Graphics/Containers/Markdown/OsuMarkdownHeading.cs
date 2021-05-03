// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using osu.Framework.Graphics.Containers.Markdown;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownHeading : MarkdownHeading
    {
        public OsuMarkdownHeading(HeadingBlock headingBlock)
            : base(headingBlock)
        {
        }

        protected override float GetFontSizeByLevel(int level)
        {
            const float base_font_size = 14;

            switch (level)
            {
                case 1:
                    return 30 / base_font_size;

                case 2:
                    return 26 / base_font_size;

                case 3:
                    return 20 / base_font_size;

                case 4:
                    return 18 / base_font_size;

                case 5:
                    return 16 / base_font_size;

                default:
                    return 1;
            }
        }
    }
}
