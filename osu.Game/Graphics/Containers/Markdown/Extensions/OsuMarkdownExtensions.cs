// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig;

namespace osu.Game.Graphics.Containers.Markdown.Extensions
{
    public static class OsuMarkdownExtensions
    {
        /// <summary>
        /// Uses the block attributes extension.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>The modified pipeline.</returns>
        public static MarkdownPipelineBuilder UseBlockAttributes(this MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.AddIfNotAlready<BlockAttributeExtension>();
            return pipeline;
        }
    }
}
