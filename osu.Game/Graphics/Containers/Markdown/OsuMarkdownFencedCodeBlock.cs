// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownFencedCodeBlock : MarkdownFencedCodeBlock
    {
        private Box background;
        private MarkdownTextFlowContainer textFlow;

        public OsuMarkdownFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
            : base(fencedCodeBlock)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            // TODO : Change to monospace font to match with osu-web
            background.Colour = colourProvider.Background6;
            textFlow.Colour = colourProvider.Light1;
        }

        protected override Drawable CreateBackground()
        {
            return background = new Box
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        public override MarkdownTextFlowContainer CreateTextFlow()
        {
            return textFlow = base.CreateTextFlow();
        }
    }
}
