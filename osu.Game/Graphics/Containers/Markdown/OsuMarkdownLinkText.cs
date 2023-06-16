// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownLinkText : MarkdownLinkText
    {
        [Resolved(canBeNull: true)]
        private ILinkHandler linkHandler { get; set; }

        private readonly string text;
        private readonly string title;

        public OsuMarkdownLinkText(string text, LinkInline linkInline)
            : base(text, linkInline)
        {
            this.text = text;
            title = linkInline.Title;
        }

        public OsuMarkdownLinkText(AutolinkInline autolinkInline)
            : base(autolinkInline)
        {
            text = autolinkInline.Url;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var textDrawable = CreateSpriteText().With(t => t.Text = text);
            var parts = new[] { textDrawable };

            InternalChildren = new Drawable[]
            {
                textDrawable,
                new OsuMarkdownLinkCompiler(parts)
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = OnLinkPressed,
                    TooltipText = title ?? Url,
                },
                new LinkFlowContainer.LinkContextMenuProvider(parts, Url, OnLinkPressed),
            };
        }

        protected override void OnLinkPressed() => linkHandler?.HandleLink(Url);

        private partial class OsuMarkdownLinkCompiler : DrawableLinkCompiler
        {
            public OsuMarkdownLinkCompiler(IEnumerable<Drawable> parts)
                : base(parts)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
            }
        }
    }
}
