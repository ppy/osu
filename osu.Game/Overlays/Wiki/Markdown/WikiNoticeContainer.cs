// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Yaml;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiNoticeContainer : FillFlowContainer
    {
        private readonly bool isOutdated;
        private readonly bool needsCleanup;

        public WikiNoticeContainer(YamlFrontMatterBlock yamlFrontMatterBlock)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            foreach (var line in yamlFrontMatterBlock.Lines)
            {
                switch (line.ToString())
                {
                    case "outdated: true":
                        isOutdated = true;
                        break;

                    case "needs_cleanup: true":
                        needsCleanup = true;
                        break;
                }
            }
        }
    }
}
