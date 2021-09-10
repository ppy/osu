// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.News
{
    public class NewsCard : OsuHoverContainer
    {
        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        private readonly APINewsPost post;

        private Box background;
        private TextFlowContainer main;

        public NewsCard(APINewsPost post)
            : base(HoverSampleSet.Submit)
        {
            this.post = post;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 6;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, GameHost host)
        {
            if (post.Slug != null)
            {
                TooltipText = "view in browser";
                Action = () => host.OpenUrlExternally("https://osu.ppy.sh/home/news/" + post.Slug);
            }

            NewsPostBackground bg;
            AddRange(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 160,
                            Masking = true,
                            CornerRadius = 6,
                            Children = new Drawable[]
                            {
                                new DelayedLoadWrapper(bg = new NewsPostBackground(post.FirstImage)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fill,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Alpha = 0
                                })
                                {
                                    RelativeSizeAxes = Axes.Both
                                },
                                new DateContainer(post.PublishedAt)
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding
                                    {
                                        Top = 10,
                                        Right = 15
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding
                            {
                                Horizontal = 15,
                                Vertical = 10
                            },
                            Child = main = new TextFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                }
            });

            IdleColour = colourProvider.Background4;
            HoverColour = colourProvider.Background3;

            bg.OnLoadComplete += d => d.FadeIn(250, Easing.In);

            main.AddParagraph(post.Title, t => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold));
            main.AddParagraph(post.Preview, t => t.Font = OsuFont.GetFont(size: 12)); // Should use sans-serif font
            main.AddParagraph("by ", t => t.Font = OsuFont.GetFont(size: 12));
            main.AddText(post.Author, t => t.Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold));
        }

        private class DateContainer : CircularContainer, IHasCustomTooltip<DateTimeOffset>
        {
            private readonly DateTimeOffset date;

            public DateContainer(DateTimeOffset date)
            {
                this.date = date;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background6.Opacity(0.5f)
                    },
                    new OsuSpriteText
                    {
                        Text = date.ToString("d MMM yyyy").ToUpper(),
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold),
                        Margin = new MarginPadding
                        {
                            Horizontal = 20,
                            Vertical = 5
                        }
                    }
                };
            }

            protected override bool OnClick(ClickEvent e) => true; // Protects the NewsCard from clicks while hovering DateContainer

            ITooltip<DateTimeOffset> IHasCustomTooltip<DateTimeOffset>.GetCustomTooltip() => new DateTooltip();

            DateTimeOffset IHasCustomTooltip<DateTimeOffset>.TooltipContent => date;
        }
    }
}
