// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osuTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class DownloadProgressBar : BeatmapDownloadTrackingComposite
    {
        private readonly ProgressBar progressBar;

        public DownloadProgressBar(BeatmapSetInfo beatmapSet)
            : base(beatmapSet)
        {
            AddInternal(progressBar = new ProgressBar
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
            progressBar.Current = Progress;

            State.BindValueChanged(state =>
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

                    case DownloadState.Downloaded:
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
