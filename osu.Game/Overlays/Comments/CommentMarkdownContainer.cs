// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax.Inlines;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Overlays.Comments
{
    public class CommentMarkdownContainer : OsuMarkdownContainer
    {
        public override MarkdownTextFlowContainer CreateTextFlow() => new CommentMarkdownTextFlowContainer();

        private class CommentMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
        {
            // Don't render image in comment for now
            protected override void AddImage(LinkInline linkInline) { }
        }
    }
}
