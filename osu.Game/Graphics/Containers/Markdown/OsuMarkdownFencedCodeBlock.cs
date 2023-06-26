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
    public partial class OsuMarkdownFencedCodeBlock : MarkdownFencedCodeBlock
    {
        // TODO : change to monospace font for this component
        public OsuMarkdownFencedCodeBlock(FencedCodeBlock fencedCodeBlock)
            : base(fencedCodeBlock)
        {
        }

        protected override Drawable CreateBackground() => new CodeBlockBackground();

        public override MarkdownTextFlowContainer CreateTextFlow() => new CodeBlockTextFlowContainer();

        private partial class CodeBlockBackground : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.Both;
                Colour = colourProvider.Background6;
            }
        }

        private partial class CodeBlockTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Light1;
                Margin = new MarginPadding(10);
            }
        }
    }
}
