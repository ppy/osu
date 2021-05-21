// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Yaml;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Wiki.Markdown
{
    public class WikiNoticeContainer : FillFlowContainer
    {
        public WikiNoticeContainer(YamlFrontMatterBlock yamlFrontMatterBlock)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
        }
    }
}
