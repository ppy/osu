// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Extensions.Yaml;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

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

            foreach (object line in yamlFrontMatterBlock.Lines)
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

        [BackgroundDependencyLoader]
        private void load()
        {
            // Reference : https://github.com/ppy/osu-web/blob/master/resources/views/wiki/_notice.blade.php and https://github.com/ppy/osu-web/blob/master/resources/lang/en/wiki.php
            // TODO : add notice box for fallback translation, legal translation and outdated translation after implement wiki locale in the future.
            if (isOutdated)
            {
                Add(new NoticeBox
                {
                    Text = "The content on this page is incomplete or outdated. If you are able to help out, please consider updating the article!",
                });
            }
            else if (needsCleanup)
            {
                Add(new NoticeBox
                {
                    Text = "This page does not meet the standards of the osu! wiki and needs to be cleaned up or rewritten. If you are able to help out, please consider updating the article!",
                });
            }
        }

        private class NoticeBox : Container
        {
            [Resolved]
            private IMarkdownTextFlowComponent parentFlowComponent { get; set; }

            public string Text { get; set; }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colour)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                MarkdownTextFlowContainer textFlow;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4,
                    },
                    textFlow = parentFlowComponent.CreateTextFlow().With(t =>
                    {
                        t.Colour = colour.Orange1;
                        t.Padding = new MarginPadding
                        {
                            Vertical = 10,
                            Horizontal = 15,
                        };
                    })
                };

                textFlow.AddText(Text);
            }
        }
    }
}
