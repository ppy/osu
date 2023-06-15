// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using Markdig.Syntax.Inlines;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
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

        private OsuMarkdownLinkCompiler markdownLink;

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
            var linkDetails = MessageFormatter.GetLinkDetails(Url);

            InternalChildren = new Drawable[]
            {
                textDrawable,
                markdownLink = new OsuMarkdownLinkCompiler(new[] { textDrawable }, linkDetails)
                {
                    RelativeSizeAxes = Axes.Both,
                    TooltipText = title ?? Url,
                }
            };
        }

        protected override void OnLinkPressed() => markdownLink.Action?.Invoke();

        private partial class OsuMarkdownLinkCompiler : DrawableLinkCompiler
        {
            public OsuMarkdownLinkCompiler(IEnumerable<Drawable> parts, LinkDetails link)
                : base(parts, link)
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
