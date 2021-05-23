// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Linq;
using HtmlAgilityPack;
using osu.Framework.Allocation;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Wiki.Markdown;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Wiki
{
    public class WikiMainPage : FillFlowContainer
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

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
            AddRange(createPanels(html));
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

        private IEnumerable<Container> createPanels(HtmlDocument html)
        {
            var panelsNode = html.DocumentNode.SelectNodes("//div[contains(@class, 'wiki-main-page-panel')]");

            foreach (var panel in panelsNode)
            {
                var isFullWidth = panel.HasClass("wiki-main-page-panel--full");

                yield return new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = isFullWidth ? 1.0f : 0.5f,
                    Padding = new MarginPadding(3),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 4,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(25),
                                Offset = new Vector2(0, 1),
                                Radius = 3,
                            },
                            Child = new Box
                            {
                                Colour = colourProvider.Background4,
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                        new WikiMarkdownContainer
                        {
                            Text = panel.InnerText,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                };
            }
        }
    }
}
