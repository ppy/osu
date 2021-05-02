// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownLinkText : MarkdownLinkText
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private SpriteText spriteText;

        public OsuMarkdownLinkText(string text, LinkInline linkInline)
            : base(text, linkInline)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            spriteText.Colour = colourProvider.Light2;
        }

        public override SpriteText CreateSpriteText()
        {
            return spriteText = base.CreateSpriteText();
        }

        protected override bool OnHover(HoverEvent e)
        {
            spriteText.Colour = colourProvider.Light1;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            spriteText.Colour = colourProvider.Light2;
            base.OnHoverLost(e);
        }
    }
}
