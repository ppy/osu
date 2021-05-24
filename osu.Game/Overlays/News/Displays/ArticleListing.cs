// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osuTK;

namespace osu.Game.Overlays.News.Displays
{
    /// <summary>
    /// Lists articles in a vertical flow for a specified year.
    /// </summary>
    public class ArticleListing : CompositeDrawable
    {
        public Action RequestMorePosts;

        private FillFlowContainer content;
        private ShowMoreButton showMore;

        private readonly GetNewsResponse initialResponse;

        /// <summary>
        /// Instantiate a listing for the specified year.
        /// </summary>
        /// <param name="initialResponse">Initial response to create articles from.</param>
        public ArticleListing(GetNewsResponse initialResponse)
        {
            this.initialResponse = initialResponse;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Vertical = 20,
                Left = 30,
                Right = 50
            };

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    content = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = initialResponse.NewsPosts.Select(p => new NewsCard(p)).ToList()
                    },
                    showMore = new ShowMoreButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding
                        {
                            Top = 15
                        },
                        Action = RequestMorePosts,
                        Alpha = initialResponse.Cursor != null ? 1 : 0
                    }
                }
            };
        }

        private CancellationTokenSource cancellationToken;

        public void AddPosts(GetNewsResponse response)
        {
            cancellationToken?.Cancel();

            LoadComponentsAsync(response.NewsPosts.Select(p => new NewsCard(p)).ToList(), loaded =>
            {
                content.AddRange(loaded);

                showMore.IsLoading = false;
                showMore.Alpha = response.Cursor != null ? 1 : 0;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
