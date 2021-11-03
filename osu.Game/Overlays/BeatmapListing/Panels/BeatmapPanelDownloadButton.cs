// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.BeatmapListing.Panels
{
    public class BeatmapPanelDownloadButton : CompositeDrawable
    {
        protected bool DownloadEnabled => button.Enabled.Value;

        /// <summary>
        /// Currently selected beatmap. Used to present the correct difficulty after completing a download.
        /// </summary>
        public readonly IBindable<APIBeatmap> SelectedBeatmap = new Bindable<APIBeatmap>();

        private readonly ShakeContainer shakeContainer;
        private readonly DownloadButton button;
        private Bindable<bool> noVideoSetting;

        protected readonly BeatmapDownloadTracker DownloadTracker;

        protected readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        private readonly IBeatmapSetInfo beatmapSet;

        public BeatmapPanelDownloadButton(IBeatmapSetInfo beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            InternalChildren = new Drawable[]
            {
                shakeContainer = new ShakeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = button = new DownloadButton
                    {
                        RelativeSizeAxes = Axes.Both,
                        State = { BindTarget = State }
                    },
                },
                DownloadTracker = new BeatmapDownloadTracker(beatmapSet)
                {
                    State = { BindTarget = State }
                }
            };

            button.Add(new DownloadProgressBar(beatmapSet)
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Depth = -1,
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, BeatmapManager beatmaps, OsuConfigManager osuConfig)
        {
            noVideoSetting = osuConfig.GetBindable<bool>(OsuSetting.PreferNoVideo);

            button.Action = () =>
            {
                switch (DownloadTracker.State.Value)
                {
                    case DownloadState.Downloading:
                    case DownloadState.Importing:
                        shakeContainer.Shake();
                        break;

                    case DownloadState.LocallyAvailable:
                        Predicate<BeatmapInfo> findPredicate = null;
                        if (SelectedBeatmap.Value != null)
                            findPredicate = b => b.OnlineBeatmapID == SelectedBeatmap.Value.OnlineID;

                        game?.PresentBeatmap(beatmapSet, findPredicate);
                        break;

                    default:
                        beatmaps.Download(beatmapSet, noVideoSetting.Value);
                        break;
                }
            };

            State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.LocallyAvailable:
                        button.Enabled.Value = true;
                        button.TooltipText = "Go to beatmap";
                        break;

                    default:
                        if ((beatmapSet as IBeatmapSetOnlineInfo)?.Availability.DownloadDisabled == true)
                        {
                            button.Enabled.Value = false;
                            button.TooltipText = "this beatmap is currently not available for download.";
                        }

                        break;
                }
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FinishTransforms(true);
        }
    }
}
