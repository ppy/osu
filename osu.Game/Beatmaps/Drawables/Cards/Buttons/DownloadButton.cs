// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class DownloadButton : BeatmapCardIconButton
    {
        public IBindable<DownloadState> State => state;
        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        private readonly APIBeatmapSet beatmapSet;

        private Bindable<bool> preferNoVideo = null!;

        private readonly LoadingSpinner spinner;

        [Resolved]
        private BeatmapModelDownloader beatmaps { get; set; } = null!;

        public DownloadButton(APIBeatmapSet beatmapSet)
        {
            Icon.Icon = FontAwesome.Solid.Download;

            Content.Add(spinner = new LoadingSpinner { Size = new Vector2(IconSize) });

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
            switch (state.Value)
            {
                case DownloadState.Downloading:
                case DownloadState.Importing:
                    Action = null;
                    TooltipText = string.Empty;
                    spinner.Show();
                    Icon.Hide();
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
                    spinner.Hide();
                    Icon.Show();

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
