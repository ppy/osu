// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using HtmlAgilityPack;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Wiki
{
    public class WikiMainPage : FillFlowContainer
    {
        public string Markdown;

        public WikiMainPage()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var html = new HtmlDocument();
            html.LoadHtml(Markdown);

            var panels = createPanels(html).ToArray();

            Children = new Drawable[]
            {
                createBlurb(html),
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.AutoSize), panels.Length).ToArray(),
                    Content = panels,
                },
            };
        }

        private Container createBlurb(HtmlDocument html)
        {
            var blurbNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'wiki-main-page__blurb')]");

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Vertical = 30,
                },
                Child = new OsuSpriteText
                {
                    Text = blurbNode.InnerText,
                    Font = OsuFont.GetFont(size: 12),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            };
        }

        private IEnumerable<Drawable[]> createPanels(HtmlDocument html)
        {
            var panelsNode = html.DocumentNode.SelectNodes("//div[contains(@class, 'wiki-main-page-panel')]").ToArray();

            Debug.Assert(panelsNode.Length > 1);

            var i = 0;

            while (i < panelsNode.Length)
            {
                var isFullWidth = panelsNode[i].HasClass("wiki-main-page-panel--full");

                if (isFullWidth)
                {
                    yield return new Drawable[]
                    {
                        new WikiPanelContainer(panelsNode[i++].InnerText, true)
                        {
                            // This is required to fill up the space of "null" drawable below.
                            Width = 2,
                        },
                        null,
                    };
                }
                else
                {
                    yield return new Drawable[]
                    {
                        new WikiPanelContainer(panelsNode[i++].InnerText),
                        i < panelsNode.Length ? new WikiPanelContainer(panelsNode[i++].InnerText) : null,
                    };
                }
            }
        }
    }
}
