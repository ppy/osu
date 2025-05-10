// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Markdig.Extensions.Footnotes;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown.Footnotes
{
    public partial class OsuMarkdownFootnoteBacklink : OsuHoverContainer
    {
        private readonly FootnoteLink backlink;

        private SpriteIcon spriteIcon = null!;

        [Resolved]
        private IMarkdownTextComponent parentTextComponent { get; set; } = null!;

        protected override IEnumerable<Drawable> EffectTargets => spriteIcon.Yield();

        public OsuMarkdownFootnoteBacklink(FootnoteLink backlink)
        {
            this.backlink = backlink;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider colourProvider, OsuMarkdownContainer markdownContainer, OverlayScrollContainer? scrollContainer)
        {
            float fontSize = parentTextComponent.CreateSpriteText().Font.Size;
            Size = new Vector2(fontSize);

            IdleColour = colourProvider.Light2;
            HoverColour = colourProvider.Light1;

            Add(spriteIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Margin = new MarginPadding { Left = 5 },
                Size = new Vector2(fontSize / 2),
                Icon = FontAwesome.Solid.ArrowUp,
            });

            if (scrollContainer != null)
            {
                Action = () =>
                {
                    var footnoteLink = markdownContainer.ChildrenOfType<OsuMarkdownFootnoteLink>().Single(footnoteLink => footnoteLink.FootnoteLink.Index == backlink.Index);
                    scrollContainer.ScrollIntoView(footnoteLink);
                };
            }
        }
    }
}
