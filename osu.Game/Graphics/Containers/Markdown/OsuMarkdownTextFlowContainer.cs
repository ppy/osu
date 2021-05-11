// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownTextFlowContainer : MarkdownTextFlowContainer
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        protected override void AddLinkText(string text, LinkInline linkInline)
            => AddDrawable(new OsuMarkdownLinkText(text, linkInline));

        // TODO : Add background (colour B6) and change font to monospace
        protected override void AddCodeInLine(CodeInline codeInline)
            => AddText(codeInline.Content, t => { t.Colour = colourProvider.Light1; });

        protected override SpriteText CreateEmphasisedSpriteText(bool bold, bool italic)
            => CreateSpriteText().With(t => t.Font = t.Font.With(weight: bold ? FontWeight.Bold : FontWeight.Regular, italics: italic));
    }
}
