// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Linq;
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

            Children = new Drawable[]
            {
                createBlurb(html)
            };
        }

        private Container createBlurb(HtmlDocument html)
        {
            var blurbNode = html.DocumentNode.SelectNodes("//div[contains(@class, 'wiki-main-page__blurb')]").First();

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
    }
}
