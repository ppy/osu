// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
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
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public DownloadButton(APIBeatmapSet beatmapSet)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Tracker = new BeatmapDownloadTracker(beatmapSet),
                Download = new DownloadIcon(beatmapSet),
                downloadProgress = new SmoothCircularProgress
                {
                    Size = new Vector2(12),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    InnerRadius = 0.4f,
                },
                Play = new PlayIcon(beatmapSet)
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
                downloadProgress.FillTo(Tracker.Progress.Value, Tracker.Progress.Value > 0 ? BeatmapCard.TRANSITION_DURATION : 0, Easing.OutQuint);

            Play.FadeTo(Tracker.State.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }

        protected class DownloadIcon : BeatmapCardIconButton
        {
            private readonly APIBeatmapSet beatmapSet;
            private Bindable<bool> preferNoVideo = null!;

            [Resolved]
            private BeatmapManager beatmaps { get; set; } = null!;

            public DownloadIcon(APIBeatmapSet beatmapSet)
            {
                Icon.Icon = FontAwesome.Solid.Download;

                this.beatmapSet = beatmapSet;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                preferNoVideo = config.GetBindable<bool>(OsuSetting.PreferNoVideo);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                preferNoVideo.BindValueChanged(_ => updateState(), true);
            }

            private void updateState()
            {
                if (beatmapSet.Availability.DownloadDisabled)
                {
                    Enabled.Value = false;
                    TooltipText = BeatmapsetsStrings.AvailabilityDisabled;
                    return;
                }

                if (!beatmapSet.HasVideo)
                    TooltipText = BeatmapsetsStrings.PanelDownloadAll;
                else
                    TooltipText = preferNoVideo.Value ? BeatmapsetsStrings.PanelDownloadNoVideo : BeatmapsetsStrings.PanelDownloadVideo;

                Action = () => beatmaps.Download(beatmapSet, preferNoVideo.Value);
            }
        }

        protected class PlayIcon : BeatmapCardIconButton
        {
            private readonly APIBeatmapSet beatmapSet;

            public PlayIcon(APIBeatmapSet beatmapSet)
            {
                this.beatmapSet = beatmapSet;

                Icon.Icon = FontAwesome.Solid.AngleDoubleRight;
                TooltipText = "Go to beatmap";
            }

            [BackgroundDependencyLoader(true)]
            private void load(OsuGame? game)
            {
                if (game != null)
                    Action = () => game.PresentBeatmap(beatmapSet);
            }
        }
    }
}
