// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.News;
using osu.Game.Overlays.News.Displays;
using osu.Game.Overlays.News.Sidebar;

namespace osu.Game.Overlays
{
    public class NewsOverlay : OnlineOverlay<NewsHeader>
    {
        private readonly Bindable<string> article = new Bindable<string>(null);

        private readonly Container sidebarContainer;
        private readonly NewsSidebar sidebar;

        private readonly Container content;

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
            article.BindValueChanged(onArticleChanged);
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
            loadFrontPage(year);
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
            LoadComponentAsync(display, loaded => content.Child = loaded, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            sidebarContainer.Height = DrawHeight;
            sidebarContainer.Y = Math.Clamp(ScrollFlow.Current - Header.DrawHeight, 0, Math.Max(ScrollFlow.ScrollContent.DrawHeight - DrawHeight - Header.DrawHeight, 0));
        }

        private void onArticleChanged(ValueChangedEvent<string> article)
        {
            if (article.NewValue == null)
                loadFrontPage();
            else
                loadArticle(article.NewValue);
        }

        private void loadFrontPage(int year = 0)
        {
            beginLoading();

            Header.SetFrontPage();

            var page = new FrontPageDisplay(year);
            page.ResponseReceived += r =>
            {
                sidebar.Metadata.Value = r.SidebarMetadata;
                Loading.Hide();
            };
            LoadDisplay(page);
        }

        private void loadArticle(string article)
        {
            beginLoading();

            Header.SetArticle(article);

            // Temporary, should be handled by ArticleDisplay later
            LoadDisplay(Empty());
            Loading.Hide();
        }

        private void beginLoading()
        {
            cancellationToken?.Cancel();
            Loading.Show();
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
