// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Wiki;

namespace osu.Game.Overlays
{
    public class WikiOverlay : OnlineOverlay<WikiHeader>
    {
        private const string index_path = @"main_page";

        private readonly Bindable<string> path = new Bindable<string>(index_path);

        private readonly Bindable<APIWikiPage> wikiData = new Bindable<APIWikiPage>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetWikiRequest request;

        private CancellationTokenSource cancellationToken;

        private bool displayUpdateRequired = true;

        private WikiArticlePage articlePage;

        public WikiOverlay()
            : base(OverlayColourScheme.Orange, false)
        {
        }

        public void ShowPage(string pagePath = index_path)
        {
            path.Value = pagePath.Trim('/');
            Show();
        }

        protected override WikiHeader CreateHeader() => new WikiHeader
        {
            ShowIndexPage = () => ShowPage(),
            ShowParentPage = showParentPage,
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            path.BindValueChanged(onPathChanged);
            wikiData.BindTo(Header.WikiPageData);
        }

        protected override void PopIn()
        {
            base.PopIn();

            if (displayUpdateRequired)
            {
                path.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            displayUpdateRequired = true;
        }

        protected void LoadDisplay(Drawable display)
        {
            ScrollFlow.ScrollToStart();
            LoadComponentAsync(display, loaded =>
            {
                Child = loaded;
                Loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (articlePage != null)
            {
                articlePage.SidebarContainer.Height = DrawHeight;
                articlePage.SidebarContainer.Y = Math.Clamp(ScrollFlow.Current - Header.DrawHeight, 0, Math.Max(ScrollFlow.ScrollContent.DrawHeight - DrawHeight - Header.DrawHeight, 0));
            }
        }

        private void onPathChanged(ValueChangedEvent<string> e)
        {
            cancellationToken?.Cancel();
            request?.Cancel();

            request = new GetWikiRequest(e.NewValue);

            Loading.Show();

            request.Success += response => Schedule(() => onSuccess(response));
            request.Failure += _ => Schedule(onFail);

            api.PerformAsync(request);
        }

        private void onSuccess(APIWikiPage response)
        {
            wikiData.Value = response;

            if (response.Layout == index_path)
            {
                LoadDisplay(new WikiMainPage
                {
                    Markdown = response.Markdown,
                    Padding = new MarginPadding
                    {
                        Vertical = 20,
                        Horizontal = 50,
                    },
                });
            }
            else
            {
                LoadDisplay(articlePage = new WikiArticlePage($@"{api.WebsiteRootUrl}/wiki/{path.Value}/", response.Markdown));
            }
        }

        private void onFail()
        {
            LoadDisplay(articlePage = new WikiArticlePage($@"{api.WebsiteRootUrl}/wiki/",
                $"Something went wrong when trying to fetch page \"{path.Value}\".\n\n[Return to the main page](Main_Page)."));
        }

        private void showParentPage()
        {
            string parentPath = string.Join("/", path.Value.Split('/').SkipLast(1));
            ShowPage(parentPath);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
