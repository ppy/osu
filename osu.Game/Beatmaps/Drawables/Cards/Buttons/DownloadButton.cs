// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Configuration;
using osu.Game.Online;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public partial class DownloadButton : BeatmapCardIconButton
    {
        public Bindable<DownloadState> State { get; } = new Bindable<DownloadState>();

        private readonly APIBeatmapSet beatmapSet;

        private Bindable<bool> preferNoVideo = null!;

        [Resolved]
        private BeatmapModelDownloader beatmaps { get; set; } = null!;

        public DownloadButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            preferNoVideo = config.GetBindable<bool>(OsuSetting.PreferNoVideo);
            Icon.Icon = FontAwesome.Solid.Download;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            preferNoVideo.BindValueChanged(_ => updateState());
            State.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            switch (State.Value)
            {
                case DownloadState.Unknown:
                    Action = null;
                    TooltipText = string.Empty;
                    break;

                case DownloadState.Downloading:
                case DownloadState.Importing:
                    Action = null;
                    TooltipText = string.Empty;
                    SetLoading(true);
                    break;

                case DownloadState.LocallyAvailable:
                    Action = null;
                    TooltipText = string.Empty;
                    this.FadeOut(BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                    break;

                case DownloadState.NotDownloaded:
                    if (beatmapSet.Availability.DownloadDisabled)
                    {
                        Enabled.Value = false;
                        TooltipText = BeatmapsetsStrings.AvailabilityDisabled;
                        return;
                    }

                    Action = () => beatmaps.Download(beatmapSet, preferNoVideo.Value);
                    this.FadeIn(BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                    SetLoading(false);

                    if (!beatmapSet.HasVideo)
                        TooltipText = BeatmapsetsStrings.PanelDownloadAll;
                    else
                        TooltipText = preferNoVideo.Value ? BeatmapsetsStrings.PanelDownloadNoVideo : BeatmapsetsStrings.PanelDownloadVideo;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown {nameof(DownloadState)} specified.");
            }
        }
    }
}
