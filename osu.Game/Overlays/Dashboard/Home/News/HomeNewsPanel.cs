// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.News;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public class HomeNewsPanel : HomePanel
    {
        private readonly APINewsPost post;

        public HomeNewsPanel(APINewsPost post)
        {
            this.post = post;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new ClickableNewsBackground(post),
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension()
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 80,
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                Anchor = Anchor.TopRight,
                                                Origin = Anchor.TopRight,
                                                RelativeSizeAxes = Axes.Y,
                                                Width = 1,
                                                Colour = ColourProvider.Light1
                                            },
                                            new Container
                                            {
                                                Anchor = Anchor.TopRight,
                                                Origin = Anchor.TopRight,
                                                AutoSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Right = 11 },
                                                Child = new DateContainer(post.PublishedAt)
                                            }
                                        }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Right = 10 },
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Margin = new MarginPadding { Top = 5, Bottom = 10 },
                                                Spacing = new Vector2(0, 10),
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new TitleLink(post),
                                                    new TextFlowContainer(f =>
                                                    {
                                                        f.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular);
                                                    })
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Text = post.Preview
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private class ClickableNewsBackground : OsuHoverContainer
        {
            private readonly APINewsPost post;

            public ClickableNewsBackground(APINewsPost post)
            {
                this.post = post;

                RelativeSizeAxes = Axes.X;
                Height = 130;
            }

            [BackgroundDependencyLoader]
            private void load(GameHost host)
            {
                NewsPostBackground bg;

                Child = new DelayedLoadWrapper(bg = new NewsPostBackground(post.FirstImage)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                })
                {
                    RelativeSizeAxes = Axes.Both
                };

                bg.OnLoadComplete += d => d.FadeIn(250, Easing.In);

                TooltipText = "view in browser";
                Action = () => host.OpenUrlExternally("https://osu.ppy.sh/home/news/" + post.Slug);

                HoverColour = Color4.White;
            }
        }

        private class TitleLink : OsuHoverContainer
        {
            private readonly APINewsPost post;

            public TitleLink(APINewsPost post)
            {
                this.post = post;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load(GameHost host)
            {
                Child = new TextFlowContainer(t =>
                {
                    t.Font = OsuFont.GetFont(weight: FontWeight.Bold);
                })
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = post.Title
                };

                TooltipText = "view in browser";
                Action = () => host.OpenUrlExternally("https://osu.ppy.sh/home/news/" + post.Slug);
            }
        }

        private class DateContainer : CompositeDrawable, IHasCustomTooltip
        {
            public ITooltip GetCustomTooltip() => new DateTooltip();

            public object TooltipContent => date;

            private readonly DateTimeOffset date;

            public DateContainer(DateTimeOffset date)
            {
                this.date = date;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold), // using Bold since there is no 800 weight alternative
                            Colour = colourProvider.Light1,
                            Text = date.Day.ToString()
                        },
                        new TextFlowContainer(f =>
                        {
                            f.Font = OsuFont.GetFont(size: 11, weight: FontWeight.Regular);
                            f.Colour = colourProvider.Light1;
                        })
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Both,
                            Text = $"{date:MMM yyyy}"
                        }
                    }
                };
            }
        }
    }
}
