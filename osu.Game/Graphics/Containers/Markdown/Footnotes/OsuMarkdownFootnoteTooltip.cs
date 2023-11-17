// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Footnotes;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown.Footnotes
{
    public partial class OsuMarkdownFootnoteTooltip : CompositeDrawable, ITooltip
    {
        private readonly FootnoteMarkdownContainer markdownContainer;

        [Cached]
        private OverlayColourProvider colourProvider;

        public OsuMarkdownFootnoteTooltip(OverlayColourProvider colourProvider)
        {
            this.colourProvider = colourProvider;

            Masking = true;
            Width = 200;
            AutoSizeAxes = Axes.Y;
            CornerRadius = 4;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6
                },
                markdownContainer = new FootnoteMarkdownContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    DocumentMargin = new MarginPadding(),
                    DocumentPadding = new MarginPadding { Horizontal = 10, Vertical = 5 }
                }
            };
        }

        public void Move(Vector2 pos) => Position = pos;

        public void SetContent(object content) => markdownContainer.SetContent((string)content);

        private partial class FootnoteMarkdownContainer : OsuMarkdownContainer
        {
            private string? lastFootnote;

            public void SetContent(string footnote)
            {
                if (footnote == lastFootnote)
                    return;

                lastFootnote = Text = footnote;
            }

            public override OsuMarkdownTextFlowContainer CreateTextFlow() => new FootnoteMarkdownTextFlowContainer();
        }

        private partial class FootnoteMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            protected override void AddFootnoteBacklink(FootnoteLink footnoteBacklink)
            {
                // we don't want footnote backlinks to show up in tooltips.
            }
        }
    }
}
