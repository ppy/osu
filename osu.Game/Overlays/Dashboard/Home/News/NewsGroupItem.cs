// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public partial class NewsGroupItem : CompositeDrawable
    {
        private readonly APINewsPost post;

        public NewsGroupItem(APINewsPost post)
        {
            this.post = post;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new GridContainer
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
                        new Box
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight,
                            Width = 1,
                            RelativeSizeAxes = Axes.Y,
                            Colour = colourProvider.Light1
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Padding = new MarginPadding { Right = 10 },
                            Child = new NewsTitleLink(post)
                        }
                    }
                }
            };
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
                TextFlowContainer textFlow;

                AutoSizeAxes = Axes.Both;
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                InternalChild = textFlow = new TextFlowContainer(t =>
                {
                    t.Colour = colourProvider.Light1;
                })
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Vertical = 5 }
                };

                textFlow.AddText(date.ToLocalisableString(@"dd"), t =>
                {
                    t.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold);
                });

                textFlow.AddText(date.ToLocalisableString(@" MMM"), t =>
                {
                    t.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Regular);
                });
            }

            ITooltip<DateTimeOffset> IHasCustomTooltip<DateTimeOffset>.GetCustomTooltip() => new DateTooltip();

            DateTimeOffset IHasCustomTooltip<DateTimeOffset>.TooltipContent => date;
        }
    }
}
