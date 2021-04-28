// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownQuoteBlock : MarkdownQuoteBlock
    {
        private Drawable background;

        public OsuMarkdownQuoteBlock(QuoteBlock quoteBlock)
            : base(quoteBlock)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Content2;
        }

        protected override Drawable CreateBackground()
        {
            return background = base.CreateBackground();
        }

        public override MarkdownTextFlowContainer CreateTextFlow()
        {
            var textFlow = base.CreateTextFlow();
            textFlow.Margin = new MarginPadding
            {
                Top = 10,
                Bottom = 10,
                Left = 20,
                Right = 20,
            };
            return textFlow;
        }
    }
}
