// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Markdig.Extensions.Footnotes;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown.Footnotes
{
    public partial class OsuMarkdownFootnoteLink : OsuHoverContainer, IHasCustomTooltip
    {
        private readonly FootnoteLink footnoteLink;

        private SpriteText spriteText = null!;

        [Resolved]
        private IMarkdownTextComponent parentTextComponent { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuMarkdownContainer markdownContainer { get; set; } = null!;

        protected override IEnumerable<Drawable> EffectTargets => spriteText.Yield();

        public OsuMarkdownFootnoteLink(FootnoteLink footnoteLink)
        {
            this.footnoteLink = footnoteLink;

            AutoSizeAxes = Axes.Both;
            Action = () => { }; // TODO
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            IdleColour = colourProvider.Light2;
            HoverColour = colourProvider.Light1;

            spriteText = parentTextComponent.CreateSpriteText();

            Add(spriteText.With(t =>
            {
                float baseSize = t.Font.Size;
                t.Font = t.Font.With(size: baseSize * 0.58f);
                t.Margin = new MarginPadding { Bottom = 0.33f * baseSize };
                t.Text = LocalisableString.Format("[{0}]", footnoteLink.Index);
            }));
        }

        public object TooltipContent
        {
            get
            {
                var span = footnoteLink.Footnote.LastChild.Span;
                return markdownContainer.Text.Substring(span.Start, span.Length);
            }
        }

        public ITooltip GetCustomTooltip() => new OsuMarkdownFootnoteTooltip(colourProvider);
    }
}
