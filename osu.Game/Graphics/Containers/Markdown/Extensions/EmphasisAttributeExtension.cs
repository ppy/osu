// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace osu.Game.Graphics.Containers.Markdown.Extensions
{
    /// <summary>
    /// Add support for generic attributes on the opening delimiter of custom inline containers, e.g. ::{ flag="XX" }::
    /// </summary>
    /// <remarks>
    /// For rationale, see implementation of <see cref="EmphasisAttributeInlineParser.PostProcess"/>.
    /// </remarks>
    public class EmphasisAttributeExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.InlineParsers.Replace<EmphasisInlineParser>(new EmphasisAttributeInlineParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }

        private class EmphasisAttributeInlineParser : EmphasisInlineParser, IPostInlineProcessor
        {
            public new bool PostProcess(InlineProcessor state, Inline? root, Inline? lastChild, int postInlineProcessorIndex, bool isFinalProcessing)
            {
                if (root is ContainerInline container)
                {
                    HtmlAttributes? lastAttributes = null;

                    // Workaround to make our use case of generic attributes in custom inline container work (e.g. ::{ flag="XX" }::)
                    // We place the attributes on the opening delimiter but EmphasisInlineParser's post process only copies the attributes from the closing delimiter to the EmphasisInline.
                    // So we copy all attributes from each Open EmphasisDelimiterInline to the following Close EmphasisDelimiterInline to make it work.
                    // See: https://github.com/xoofx/markdig/blob/56e9c238584a44a169f174c881855c049768634c/src/Markdig/Parsers/Inlines/EmphasisInlineParser.cs#L346-L351
                    foreach (var delimiterInline in container.FindDescendants<EmphasisDelimiterInline>())
                    {
                        if (delimiterInline.DelimiterChar == ':' && delimiterInline.DelimiterCount == 2)
                        {
                            switch (delimiterInline.Type)
                            {
                                case DelimiterType.Open:
                                    lastAttributes = delimiterInline.TryGetAttributes();
                                    break;

                                case DelimiterType.Close:
                                    if (lastAttributes != null)
                                    {
                                        delimiterInline.SetAttributes(lastAttributes);
                                        lastAttributes = null;
                                    }

                                    break;
                            }
                        }
                    }
                }

                return base.PostProcess(state, root, lastChild, postInlineProcessorIndex, isFinalProcessing);
            }
        }
    }
}
