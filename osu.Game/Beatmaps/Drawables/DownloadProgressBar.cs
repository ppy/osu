// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class DownloadProgressBar : CompositeDrawable
    {
        private readonly ProgressBar progressBar;
        private readonly BeatmapDownloadTracker downloadTracker;

        public DownloadProgressBar(IBeatmapSetInfo beatmapSet)
        {
            InternalChildren = new Drawable[]
            {
                progressBar = new ProgressBar(false)
                {
                    Height = 0,
                    Alpha = 0,
                },
                downloadTracker = new BeatmapDownloadTracker(beatmapSet),
            };
            AddInternal(progressBar = new ProgressBar(false)
            {
                Height = 0,
                Alpha = 0,
            });

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours)
        {
            progressBar.FillColour = colours.Blue;
            progressBar.BackgroundColour = Color4.Black.Opacity(0.7f);
            progressBar.Current.BindTarget = downloadTracker.Progress;

            downloadTracker.State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.NotDownloaded:
                        progressBar.Current.Value = 0;
                        progressBar.FadeOut(500);
                        break;

                    case DownloadState.Downloading:
                        progressBar.FadeIn(400, Easing.OutQuint);
                        progressBar.ResizeHeightTo(4, 400, Easing.OutQuint);
                        break;

                    case DownloadState.Importing:
                        progressBar.FadeIn(400, Easing.OutQuint);
                        progressBar.ResizeHeightTo(4, 400, Easing.OutQuint);

                        progressBar.Current.Value = 1;
                        progressBar.FillColour = colours.Yellow;
                        break;

                    case DownloadState.LocallyAvailable:
                        progressBar.FadeOut(500);
                        break;
                }
            }, true);
        }
    }
}
