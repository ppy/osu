// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownHeading : MarkdownHeading
    {
        private readonly int level;

        public OsuMarkdownHeading(HeadingBlock headingBlock)
            : base(headingBlock)
        {
            level = headingBlock.Level;
        }

        public override MarkdownTextFlowContainer CreateTextFlow() => new HeadingTextFlowContainer
        {
            FontSize = GetFontSizeByLevel(level),
            FontWeight = GetFontWeightByLevel(level),
        };

        protected override float GetFontSizeByLevel(int level)
        {
            // Reference for this font size
            // https://github.com/ppy/osu-web/blob/376cac43a051b9c85ce95e2c446099be187b3e45/resources/assets/less/bem/osu-md.less#L9
            // https://github.com/ppy/osu-web/blob/376cac43a051b9c85ce95e2c446099be187b3e45/resources/assets/less/variables.less#L161
            switch (level)
            {
                case 1:
                    return 30;

                case 2:
                    return 26;

                case 3:
                    return 20;

                case 4:
                    return 18;

                case 5:
                    return 16;

                default:
                    return 14;
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

        private partial class HeadingTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            public float FontSize;
            public FontWeight FontWeight;

            protected override SpriteText CreateSpriteText()
                => base.CreateSpriteText().With(t => t.Font = t.Font.With(Typeface.Torus, size: FontSize, weight: FontWeight));
        }
    }
}
