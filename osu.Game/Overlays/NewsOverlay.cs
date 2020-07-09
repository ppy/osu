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
                            header = new NewsHeader(),
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
            header.Post.BindValueChanged(onPostChanged, true);
        }

        public void ShowFrontPage()
        {
            header.SetFrontPage();
            Show();
        }

        public void ShowArticle(string slug)
        {
            header.SetArticle(slug);
            Show();
        }

        private CancellationTokenSource cancellationToken;

        private void onPostChanged(ValueChangedEvent<string> post)
        {
            cancellationToken?.Cancel();
            loading.Show();

            if (post.NewValue == NewsHeader.FRONT_PAGE_STRING)
            {
                LoadDisplay(new FrontPageDisplay());
                return;
            }

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
