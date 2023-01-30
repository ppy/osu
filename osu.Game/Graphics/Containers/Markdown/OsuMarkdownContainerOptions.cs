// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Footnotes;
using osu.Game.Graphics.Containers.Markdown.Extensions;

namespace osu.Game.Graphics.Containers.Markdown
{
    /// <summary>
    /// Groups options of customising the set of available extensions to <see cref="OsuMarkdownContainer"/> instances.
    /// </summary>
    public class OsuMarkdownContainerOptions
    {
        /// <summary>
        /// Allows the <see cref="OsuMarkdownContainer"/> to parse and link footnotes.
        /// </summary>
        /// <seealso cref="FootnoteExtension"/>
        public bool Footnotes { get; init; }

        /// <summary>
        /// Allows the <see cref="OsuMarkdownContainer"/> container to make URL text clickable.
        /// </summary>
        /// <seealso cref="AutoLinkExtension"/>
        public bool Autolinks { get; init; }

        /// <summary>
        /// Allows the <see cref="OsuMarkdownContainer"/> to parse custom containers (used for flags and infoboxes).
        /// </summary>
        /// <seealso cref="CustomContainerExtension"/>
        public bool CustomContainers { get; init; }

        /// <summary>
        /// Allows the <see cref="OsuMarkdownContainer"/> to parse custom attributes in block elements (used e.g. for custom anchor names in the wiki).
        /// </summary>
        /// <seealso cref="BlockAttributeExtension"/>
        public bool BlockAttributes { get; init; }

        /// <summary>
        /// Returns a prepared <see cref="MarkdownPipeline"/> according to the options specified by the current <see cref="OsuMarkdownContainerOptions"/> instance.
        /// </summary>
        /// <remarks>
        /// Compare: https://github.com/ppy/osu-web/blob/05488a96b25b5a09f2d97c54c06dd2bae59d1dc8/app/Libraries/Markdown/OsuMarkdown.php#L301
        /// </remarks>
        public MarkdownPipeline BuildPipeline()
        {
            var pipeline = new MarkdownPipelineBuilder()
                           .UseAutoIdentifiers()
                           .UsePipeTables()
                           .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                           .UseYamlFrontMatter();

            if (Footnotes)
                pipeline = pipeline.UseFootnotes();

            if (Autolinks)
                pipeline = pipeline.UseAutoLinks();

            if (CustomContainers)
                pipeline = pipeline.UseCustomContainers();

            if (BlockAttributes)
                pipeline = pipeline.UseBlockAttributes();

            return pipeline.Build();
        }
    }
}
