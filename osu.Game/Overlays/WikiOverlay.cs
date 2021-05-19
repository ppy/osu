// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Wiki;
using osu.Game.Overlays.Wiki.Markdown;

namespace osu.Game.Overlays
{
    public class WikiOverlay : OnlineOverlay<WikiHeader>
    {
        private const string index_path = "Main_Page";

        private readonly Bindable<string> path = new Bindable<string>(index_path);

        [Cached]
        private readonly Bindable<APIWikiPage> wikiData = new Bindable<APIWikiPage>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetWikiRequest request;

        private CancellationTokenSource cancellationToken;

        private bool displayUpdateRequired = true;

        public WikiOverlay()
            : base(OverlayColourScheme.Orange, false)
        {
        }

        private void onPathChanged(ValueChangedEvent<string> e)
        {
            cancellationToken?.Cancel();
            request?.Cancel();

            request = new GetWikiRequest(e.NewValue);

            Loading.Show();

            request.Success += response => Schedule(() => onSuccess(response));

            api.PerformAsync(request);
        }

        private void onSuccess(APIWikiPage response)
        {
            wikiData.Value = response;
            LoadDisplay(new WikiMarkdownContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                CurrentPath = $"{path.Value}/",
                Text = response.Markdown,
            });
        }

        public void ShowPage(string pagePath = index_path)
        {
            path.Value = pagePath.Trim('/');
            Show();
        }

        protected override WikiHeader CreateHeader() => new WikiHeader();

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

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
