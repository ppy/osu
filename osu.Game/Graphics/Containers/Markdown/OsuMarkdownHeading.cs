// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Syntax;
using osu.Framework.Graphics.Containers.Markdown;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownHeading : MarkdownHeading
    {
        public OsuMarkdownHeading(HeadingBlock headingBlock)
            : base(headingBlock)
        {
        }
    }
}
