// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
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
    public partial class FeaturedNewsItemPanel : HomePanel
    {
        private readonly APINewsPost post;

        public FeaturedNewsItemPanel(APINewsPost post)
        {
            this.post = post;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
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
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, size: 60),
                                new Dimension(GridSizeMode.Absolute, size: 20),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new Date(post.PublishedAt),
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Vertical = 10 },
                                        Child = new Box
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopRight,
                                            Width = 1,
                                            RelativeSizeAxes = Axes.Y,
                                            Colour = colourProvider.Light1
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 5, Bottom = 10 },
                                        Padding = new MarginPadding { Right = 10 },
                                        Spacing = new Vector2(0, 10),
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new NewsTitleLink(post),
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
            };
        }

        private partial class ClickableNewsBackground : OsuHoverContainer
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
                Child = new DelayedLoadUnloadWrapper(() => new NewsPostBackground(post.FirstImage)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                })
                {
                    RelativeSizeAxes = Axes.Both
                };

                TooltipText = "view in browser";
                Action = () => host.OpenUrlExternally("https://osu.ppy.sh/home/news/" + post.Slug);

                HoverColour = Color4.White;
            }
        }

        private partial class Date : CompositeDrawable, IHasCustomTooltip<DateTimeOffset>
        {
            private readonly DateTimeOffset date;

            public Date(DateTimeOffset date)
            {
                this.date = date;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                Margin = new MarginPadding { Top = 10 };
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
                            Text = date.ToLocalisableString(@"dd")
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
                            Text = date.ToLocalisableString(@"MMM yyyy")
                        }
                    }
                };
            }

            ITooltip<DateTimeOffset> IHasCustomTooltip<DateTimeOffset>.GetCustomTooltip() => new DateTooltip();

            DateTimeOffset IHasCustomTooltip<DateTimeOffset>.TooltipContent => date;
        }
    }
}
