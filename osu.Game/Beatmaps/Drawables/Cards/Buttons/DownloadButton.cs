// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Overlays;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class DownloadButton : CompositeDrawable
    {
        protected readonly DownloadIcon Download;
        protected readonly PlayIcon Play;
        protected readonly BeatmapDownloadTracker Tracker;

        private readonly SmoothCircularProgress downloadProgress;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public DownloadButton(APIBeatmapSet beatmapSet)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Tracker = new BeatmapDownloadTracker(beatmapSet),
                Download = new DownloadIcon(),
                downloadProgress = new SmoothCircularProgress
                {
                    Size = new Vector2(12),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    InnerRadius = 0.4f,
                },
                Play = new PlayIcon()
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Tracker.Progress.BindValueChanged(_ => updateState());
            Tracker.State.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            Download.FadeTo(Tracker.State.Value == DownloadState.NotDownloaded ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            downloadProgress.FadeTo(Tracker.State.Value == DownloadState.Downloading || Tracker.State.Value == DownloadState.Importing ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            downloadProgress.FadeColour(Tracker.State.Value == DownloadState.Importing ? colours.Yellow : colourProvider.Highlight1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            if (Tracker.State.Value == DownloadState.Downloading)
                downloadProgress.FillTo(Tracker.Progress.Value, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            Play.FadeTo(Tracker.State.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }

        protected class DownloadIcon : BeatmapCardIconButton
        {
            public DownloadIcon()
            {
                Icon.Icon = FontAwesome.Solid.Download;
            }
        }

        protected class PlayIcon : BeatmapCardIconButton
        {
            public PlayIcon()
            {
                Icon.Icon = FontAwesome.Regular.PlayCircle;
            }
        }

        // TODO: implement behaviour
    }
}
