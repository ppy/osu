// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.News;

namespace osu.Game.Overlays
{
    public class NewsOverlay : FullscreenOverlay
    {
        private NewsHeader header;

        private Container<NewsContent> content;

        public readonly Bindable<string> Current = new Bindable<string>(null);

        public NewsOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.PurpleDarkAlternative
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
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
                            content = new Container<NewsContent>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            }
                        },
                    },
                },
            };

            header.Post.BindTo(Current);
            Current.TriggerChange();
        }

        private CancellationTokenSource loadContentCancellation;

        protected void LoadAndShowContent(NewsContent newContent)
        {
            content.FadeTo(0.2f, 300, Easing.OutQuint);

            loadContentCancellation?.Cancel();

            LoadComponentAsync(newContent, c =>
            {
                content.Child = c;
                content.FadeIn(300, Easing.OutQuint);
            }, (loadContentCancellation = new CancellationTokenSource()).Token);
        }

        public void ShowFrontPage()
        {
            Current.Value = null;
            Show();
        }
    }
}
