// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownImage : MarkdownImage, IHasTooltip
    {
        public LocalisableString TooltipText { get; }

        public OsuMarkdownImage(LinkInline linkInline)
            : base(linkInline.Url)
        {
            TooltipText = linkInline.Title;
        }
    }
}
