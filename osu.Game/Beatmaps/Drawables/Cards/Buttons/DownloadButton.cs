// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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
    public class DownloadButton : BeatmapCardIconButton
    {
        public IBindable<DownloadState> State => state;
        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        private readonly APIBeatmapSet beatmapSet;

        private Bindable<bool> preferNoVideo = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        public DownloadButton(APIBeatmapSet beatmapSet)
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
            preferNoVideo.BindValueChanged(_ => updateState());
            state.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            this.FadeTo(state.Value != DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

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
}
