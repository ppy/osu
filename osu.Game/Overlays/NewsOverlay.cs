// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.News;
using osu.Game.Overlays.News.Displays;
using osu.Game.Overlays.News.Sidebar;

namespace osu.Game.Overlays
{
    public partial class NewsOverlay : OnlineOverlay<NewsHeader>
    {
        private readonly Bindable<string> article = new Bindable<string>();

        private readonly Container sidebarContainer;
        private readonly NewsSidebar sidebar;
        private readonly Container content;

        private GetNewsRequest request;

        private Cursor lastCursor;

        /// <summary>
        /// The year currently being displayed. If null, the main listing is being displayed.
        /// </summary>
        private int? displayedYear;

        private CancellationTokenSource cancellationToken;

        private bool displayUpdateRequired = true;

        public NewsOverlay()
            : base(OverlayColourScheme.Purple, false)
        {
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        sidebarContainer = new Container
                        {
                            AutoSizeAxes = Axes.X,
                            Child = sidebar = new NewsSidebar()
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // should not be run until first pop-in to avoid requesting data before user views.
            article.BindValueChanged(a =>
            {
                if (a.NewValue == null)
                    loadListing();
                else
                    loadArticle(a.NewValue);
            });
        }

        protected override NewsHeader CreateHeader() => new NewsHeader { ShowFrontPage = ShowFrontPage };

        protected override void PopIn()
        {
            base.PopIn();

            if (displayUpdateRequired)
            {
                article.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            displayUpdateRequired = true;
        }

        public void ShowFrontPage()
        {
            article.Value = null;
            Show();
        }

        public void ShowYear(int year)
        {
            loadListing(year);
            Show();
        }

        public void ShowArticle(string slug)
        {
            article.Value = slug;
            Show();
        }

        protected void LoadDisplay(Drawable display)
        {
            ScrollFlow.ScrollToStart();
            LoadComponentAsync(display, loaded =>
            {
                content.Child = loaded;
                Loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            sidebarContainer.Height = DrawHeight;
            sidebarContainer.Y = Math.Clamp(ScrollFlow.Current - Header.DrawHeight, 0, Math.Max(ScrollFlow.ScrollContent.DrawHeight - DrawHeight - Header.DrawHeight, 0));
        }

        private void loadListing(int? year = null)
        {
            Header.SetFrontPage();

            displayedYear = year;
            lastCursor = null;

            beginLoading(true);

            request = new GetNewsRequest(displayedYear);
            request.Success += response => Schedule(() =>
            {
                lastCursor = response.Cursor;
                sidebar.Metadata.Value = response.SidebarMetadata;

                var listing = new ArticleListing(getMorePosts);
                listing.AddPosts(response.NewsPosts, response.Cursor != null);
                LoadDisplay(listing);
            });

            API.PerformAsync(request);
        }

        private void getMorePosts()
        {
            beginLoading(false);

            request = new GetNewsRequest(displayedYear, lastCursor);
            request.Success += response => Schedule(() =>
            {
                lastCursor = response.Cursor;
                if (content.Child is ArticleListing listing)
                    listing.AddPosts(response.NewsPosts, response.Cursor != null);
            });

            API.PerformAsync(request);
        }

        private void loadArticle(string article)
        {
            // This is not yet implemented nor called from anywhere.
            beginLoading(true);

            Header.SetArticle(article);
            LoadDisplay(Empty());
        }

        private void beginLoading(bool showLoadingOverlay)
        {
            request?.Cancel();
            cancellationToken?.Cancel();

            if (showLoadingOverlay)
                Loading.Show();
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
