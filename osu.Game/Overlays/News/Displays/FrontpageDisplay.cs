// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osuTK;

namespace osu.Game.Overlays.News.Displays
{
    public class FrontpageDisplay : CompositeDrawable
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly FillFlowContainer content;
        private readonly FrontpageShowMoreButton showMore;

        private GetNewsRequest request;
        private Cursor lastCursor;

        public FrontpageDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Top = 20,
                Bottom = 10,
                Left = 35,
                Right = 55
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
                    showMore = new FrontpageShowMoreButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding
                        {
                            Vertical = 15
                        },
                        Action = fetchPage,
                        Alpha = 0
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            fetchPage();
        }

        private void fetchPage()
        {
            request = new GetNewsRequest(lastCursor);
            request.Success += response => Schedule(() => createContent(response));
            api.PerformAsync(request);
        }

        private CancellationTokenSource cancellationToken;

        private void createContent(GetNewsResponse response)
        {
            lastCursor = response.Cursor;

            var flow = new FillFlowContainer<NewsCard>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10)
            };

            response.NewsPosts.ForEach(p =>
            {
                flow.Add(new NewsCard(p));
            });

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

        private class FrontpageShowMoreButton : ShowMoreButton
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Height = 20;

                IdleColour = colourProvider.Background3;
                HoverColour = colourProvider.Background2;
                ChevronIconColour = colourProvider.Foreground1;
            }
        }
    }
}
