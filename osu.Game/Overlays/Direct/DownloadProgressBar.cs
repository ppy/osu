// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class DownloadProgressBar : DownloadTrackingComponent
    {
        private readonly ProgressBar progressBar;

        private OsuColour colours;

        public DownloadProgressBar(BeatmapSetInfo setInfo)
            : base(setInfo)
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
            this.colours = colours;

            progressBar.FillColour = colours.Blue;
            progressBar.BackgroundColour = Color4.Black.Opacity(0.7f);
        }

        protected override void DownloadFailed()
        {
            progressBar.Current.Value = 0;
            progressBar.FadeOut(500);
        }

        protected override void DownloadComleted()
        {
            progressBar.Current.Value = 1;
            progressBar.FillColour = colours.Yellow;
        }

        protected override void DownloadStarted()
        {
            progressBar.FadeIn(400, Easing.OutQuint);
            progressBar.ResizeHeightTo(4, 400, Easing.OutQuint);
        }

        protected override void BeatmapImported()
        {
            progressBar.FadeOut(500);
        }

        protected override void ProgressChanged(float progress)
        {
            progressBar.Current.Value = progress;
        }
    }
}
