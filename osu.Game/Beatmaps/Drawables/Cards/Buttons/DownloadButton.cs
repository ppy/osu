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

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class DownloadButton : CompositeDrawable
    {
        protected readonly DownloadIcon Download;
        protected readonly PlayIcon Play;
        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

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
                Download = new DownloadIcon(beatmapSet),
                Play = new PlayIcon(beatmapSet)
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapDownloadTracker? tracker)
        {
            if (tracker != null)
                ((IBindable<DownloadState>)State).BindTo(tracker.State);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            Download.FadeTo(State.Value != DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            Download.Enabled.Value = State.Value == DownloadState.NotDownloaded;

            Play.FadeTo(State.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
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
