// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownLinkText : MarkdownLinkText
    {
        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        private readonly string? title;

        public OsuMarkdownLinkText(LinkInline linkInline)
            : base(linkInline)
        {
            title = linkInline.Title;
        }

        public OsuMarkdownLinkText(AutolinkInline autolinkInline, bool bold, bool italic)
            : base(autolinkInline, bold, italic)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var content = CreateContent();

            InternalChildren = new Drawable[]
            {
                content,
                new OsuMarkdownLinkCompiler(content)
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = OnLinkPressed,
                    TooltipText = title ?? Url,
                }
            };
        }

        protected override void OnLinkPressed() => linkHandler?.HandleLink(Url);

        private partial class OsuMarkdownLinkCompiler : DrawableLinkCompiler
        {
            private readonly Drawable content;

            public OsuMarkdownLinkCompiler(Drawable content)
                : base(new[] { content })
            {
                this.content = content;
            }

            protected override IEnumerable<Drawable> EffectTargets => content.ChildrenOfType<SpriteText>();

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
            }
        }
    }
}
