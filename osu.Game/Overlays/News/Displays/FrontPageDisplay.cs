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
    public class FrontPageDisplay : CompositeDrawable
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private FillFlowContainer content;
        private ShowMoreButton showMore;

        private GetNewsRequest request;
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
                        Action = performFetch,
                        Alpha = 0
                    }
                }
            };

            performFetch();
        }

        private void performFetch()
        {
            request?.Cancel();

            request = new GetNewsRequest(lastCursor);
            request.Success += response => Schedule(() => onSuccess(response));
            api.PerformAsync(request);
        }

        private CancellationTokenSource cancellationToken;

        private void onSuccess(GetNewsResponse response)
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

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
