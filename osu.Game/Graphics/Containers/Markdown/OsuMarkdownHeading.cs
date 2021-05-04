// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownHeading : MarkdownHeading
    {
        private readonly int level;

        public OsuMarkdownHeading(HeadingBlock headingBlock)
            : base(headingBlock)
        {
            level = headingBlock.Level;
        }

        public override MarkdownTextFlowContainer CreateTextFlow() => new HeadingTextFlowContainer
        {
            Weight = GetFontWeightByLevel(level),
        };

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

        protected virtual FontWeight GetFontWeightByLevel(int level)
        {
            switch (level)
            {
                case 1:
                case 2:
                    return FontWeight.SemiBold;

                default:
                    return FontWeight.Bold;
            }
        }

        private class HeadingTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            public FontWeight Weight { get; set; }

            protected override SpriteText CreateSpriteText()
            {
                var spriteText = base.CreateSpriteText();
                spriteText.Font = spriteText.Font.With(weight: Weight);
                return spriteText;
            }
        }
    }
}
