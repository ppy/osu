// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers.Markdown;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownTextFlowContainer : MarkdownTextFlowContainer
    {
        protected override void AddLinkText(string text, LinkInline linkInline)
            => AddDrawable(new OsuMarkdownLinkText(text, linkInline));
    }
}
