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

namespace osu.Game.Overlays
{
    public class NewsOverlay : FullscreenOverlay
    {
        public readonly Bindable<string> Current = new Bindable<string>(null);

        private Container content;
        private LoadingLayer loading;
        private OverlayScrollContainer scrollFlow;

        public NewsOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            NewsHeader header;

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

            header.Post.BindTo(Current);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(onCurrentChanged, true);
        }

        public void ShowFrontPage()
        {
            Current.Value = null;
            Show();
        }

        private CancellationTokenSource cancellationToken;

        private void onCurrentChanged(ValueChangedEvent<string> current)
        {
            cancellationToken?.Cancel();
            loading.Show();

            if (current.NewValue == null)
            {
                LoadDisplay(Empty());
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
