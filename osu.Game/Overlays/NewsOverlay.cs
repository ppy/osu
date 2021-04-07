// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.News;
using osu.Game.Overlays.News.Displays;

namespace osu.Game.Overlays
{
    public class NewsOverlay : OnlineOverlay<NewsHeader>
    {
        private readonly Bindable<string> article = new Bindable<string>(null);

        public NewsOverlay()
            : base(OverlayColourScheme.Purple, false)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // should not be run until first pop-in to avoid requesting data before user views.
            article.BindValueChanged(onArticleChanged);
        }

        protected override NewsHeader CreateHeader() => new NewsHeader
        {
            ShowFrontPage = ShowFrontPage
        };

        private bool displayUpdateRequired = true;

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

        public void ShowArticle(string slug)
        {
            article.Value = slug;
            Show();
        }

        private CancellationTokenSource cancellationToken;

        private void onArticleChanged(ValueChangedEvent<string> e)
        {
            cancellationToken?.Cancel();
            Loading.Show();

            if (e.NewValue == null)
            {
                Header.SetFrontPage();
                LoadDisplay(new FrontPageDisplay());
                return;
            }

            Header.SetArticle(e.NewValue);
            LoadDisplay(Empty());
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
            base.Dispose(isDisposing);
        }
    }
}
