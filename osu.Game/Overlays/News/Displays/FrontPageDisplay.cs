// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osuTK;

namespace osu.Game.Overlays.News.Displays
{
    public class FrontPageDisplay : OverlayView<GetNewsResponse>
    {
        protected override bool PerformFetchOnApiStateChange => false;

        protected override APIRequest<GetNewsResponse> CreateRequest() => new GetNewsRequest();

        private FillFlowContainer content;
        private ShowMoreButton showMore;

        private GetNewsRequest olderPostsRequest;
        private Cursor lastCursor;

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
                        Spacing = new Vector2(0, 10)
                    },
                    showMore = new ShowMoreButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding
                        {
                            Top = 15
                        },
                        Action = fetchOlderPosts,
                        Alpha = 0
                    }
                }
            };
        }

        private CancellationTokenSource cancellationToken;

        protected override void OnSuccess(GetNewsResponse response)
        {
            cancellationToken?.Cancel();

            lastCursor = response.Cursor;

            var flow = new FillFlowContainer<NewsCard>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = response.NewsPosts.Select(p => new NewsCard(p)).ToList()
            };

            LoadComponentAsync(flow, loaded =>
            {
                content.Add(loaded);
                showMore.IsLoading = false;
                showMore.Show();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private void fetchOlderPosts()
        {
            olderPostsRequest?.Cancel();

            olderPostsRequest = new GetNewsRequest(lastCursor);
            olderPostsRequest.Success += response => Schedule(() => OnSuccess(response));
            API.PerformAsync(olderPostsRequest);
        }

        protected override void Dispose(bool isDisposing)
        {
            olderPostsRequest?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
