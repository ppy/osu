// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.News.Displays
{
    /// <summary>
    /// Lists articles in a vertical flow for a specified year.
    /// </summary>
    public partial class ArticleListing : CompositeDrawable
    {
        private readonly Action fetchMorePosts;

        private FillFlowContainer content;
        private ShowMoreButton showMore;

        private CancellationTokenSource cancellationToken;

        public ArticleListing(Action fetchMorePosts)
        {
            this.fetchMorePosts = fetchMorePosts;
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
                Right = WaveOverlayContainer.HORIZONTAL_PADDING
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
                        Spacing = new Vector2(0, 10)
                    },
                    showMore = new ShowMoreButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 15 },
                        Action = fetchMorePosts,
                        Alpha = 0
                    }
                }
            };
        }

        public void AddPosts(IEnumerable<APINewsPost> posts, bool morePostsAvailable) => Schedule(() =>
            LoadComponentsAsync(posts.Select(p => new NewsCard(p)).ToList(), loaded =>
            {
                content.AddRange(loaded);
                showMore.IsLoading = false;
                showMore.Alpha = morePostsAvailable ? 1 : 0;
            }, (cancellationToken = new CancellationTokenSource()).Token)
        );

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
