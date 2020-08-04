// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.News;
using osu.Game.Overlays.News.Displays;

namespace osu.Game.Overlays
{
    public class NewsOverlay : FullscreenOverlay
    {
        private readonly Bindable<string> article = new Bindable<string>(null);

        private Container content;
        private LoadingLayer loading;
        private NewsHeader header;
        private OverlayScrollContainer scrollFlow;

        public NewsOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background5,
                },
                scrollFlow = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new NewsHeader
                            {
                                ShowFrontPage = ShowFrontPage
                            },
                            content = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            }
                        },
                    },
                },
                loading = new LoadingLayer(content),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            article.BindValueChanged(onArticleChanged, true);
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
            loading.Show();

            if (e.NewValue == null)
            {
                header.SetFrontPage();
                LoadDisplay(new FrontPageDisplay());
                return;
            }

            header.SetArticle(e.NewValue);
            LoadDisplay(Empty());
        }

        protected void LoadDisplay(Drawable display)
        {
            scrollFlow.ScrollToStart();
            LoadComponentAsync(display, loaded =>
            {
                content.Child = loaded;
                loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
