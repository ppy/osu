// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;

namespace osu.Game.Overlays.BeatmapListing.Panels
{
    public class BeatmapPanelDownloadButton : BeatmapDownloadTrackingComposite
    {
        protected bool DownloadEnabled => button.Enabled.Value;

        /// <summary>
        /// Currently selected beatmap. Used to present the correct difficulty after completing a download.
        /// </summary>
        public readonly IBindable<BeatmapInfo> SelectedBeatmap = new Bindable<BeatmapInfo>();

        private readonly ShakeContainer shakeContainer;
        private readonly DownloadButton button;
        private Bindable<bool> noVideoSetting;

        public BeatmapPanelDownloadButton(BeatmapSetInfo beatmapSet)
            : base(beatmapSet)
        {
            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new DownloadButton
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            button.State.BindTo(State);
            FinishTransforms(true);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, BeatmapManager beatmaps, OsuConfigManager osuConfig)
        {
            noVideoSetting = osuConfig.GetBindable<bool>(OsuSetting.PreferNoVideo);

            button.Action = () =>
            {
                switch (State.Value)
                {
                    case DownloadState.Downloading:
                    case DownloadState.Downloaded:
                        shakeContainer.Shake();
                        break;

                    case DownloadState.LocallyAvailable:
                        Predicate<BeatmapInfo> findPredicate = null;
                        if (SelectedBeatmap.Value != null)
                            findPredicate = b => b.OnlineBeatmapID == SelectedBeatmap.Value.OnlineBeatmapID;

                        game?.PresentBeatmap(BeatmapSet.Value, findPredicate);
                        break;

                    default:
                        beatmaps.Download(BeatmapSet.Value, noVideoSetting.Value);
                        break;
                }
            };

            State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.LocallyAvailable:
                        button.Enabled.Value = true;
                        button.TooltipText = string.Empty;
                        break;

                    default:
                        if (BeatmapSet.Value?.OnlineInfo?.Availability?.DownloadDisabled ?? false)
                        {
                            button.Enabled.Value = false;
                            button.TooltipText = "this beatmap is currently not available for download.";
                        }

                        break;
                }
            }, true);
        }
    }
}
