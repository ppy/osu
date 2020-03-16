// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays.News
{
    public class NewsArticleCover : Container
    {
        private const int hover_duration = 300;

        private readonly Box gradient;

        public NewsArticleCover(ArticleInfo info)
        {
            RelativeSizeAxes = Axes.X;
            Masking = true;
            CornerRadius = 4;

            NewsBackground bg;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.1f))
                },
                new DelayedLoadWrapper(bg = new NewsBackground(info.CoverUrl)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Alpha = 0
                })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                gradient = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.7f)),
                    Alpha = 0
                },
                new DateContainer(info.Time)
                {
                    Margin = new MarginPadding
                    {
                        Right = 20,
                        Top = 20,
                    }
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = new MarginPadding
                    {
                        Left = 25,
                        Bottom = 50,
                    },
                    Font = OsuFont.GetFont(Typeface.Torus, 24, FontWeight.Bold),
                    Text = info.Title,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = new MarginPadding
                    {
                        Left = 25,
                        Bottom = 30,
                    },
                    Font = OsuFont.GetFont(Typeface.Torus, 16, FontWeight.Bold),
                    Text = "by " + info.Author
                }
            };

            bg.OnLoadComplete += d => d.FadeIn(250, Easing.In);
        }

        protected override bool OnHover(HoverEvent e)
        {
            gradient.FadeIn(hover_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            gradient.FadeOut(hover_duration, Easing.OutQuint);
        }

        [LongRunningLoad]
        private class NewsBackground : Sprite
        {
            private readonly string url;

            public NewsBackground(string coverUrl)
            {
                url = coverUrl ?? "Headers/news";
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore store)
            {
                Texture = store.Get(url);
            }
        }

        private class DateContainer : Container, IHasTooltip
        {
            private readonly DateTime date;

            public DateContainer(DateTime date)
            {
                this.date = date;

                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                Masking = true;
                CornerRadius = 4;
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(Typeface.Torus, 12, FontWeight.Black, false, false),
                        Text = date.ToString("d MMM yyy").ToUpper(),
                        Margin = new MarginPadding
                        {
                            Vertical = 4,
                            Horizontal = 8,
                        }
                    }
                };
            }

            public string TooltipText => date.ToString("dddd dd MMMM yyyy hh:mm:ss UTCz").ToUpper();
        }

        //fake API data struct to use for now as a skeleton for data, as there is no API struct for news article info for now
        public class ArticleInfo
        {
            public string Title { get; set; }
            public string CoverUrl { get; set; }
            public DateTime Time { get; set; }
            public string Author { get; set; }
        }
    }
}
