// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public abstract class HomeNewsPanelFooter : CompositeDrawable
    {
        protected virtual float BarPadding { get; } = 0;

        private readonly APINewsPost post;

        protected HomeNewsPanelFooter(APINewsPost post)
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
                        CreateDate(post.PublishedAt),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = BarPadding },
                            Child = new Box
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopRight,
                                Width = 1,
                                RelativeSizeAxes = Axes.Y,
                                Colour = colourProvider.Light1
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Padding = new MarginPadding { Right = 10 },
                            Child = CreateContent(post)
                        }
                    }
                }
            };
        }

        protected abstract Drawable CreateDate(DateTimeOffset date);

        protected abstract Drawable CreateContent(APINewsPost post);
    }
}
