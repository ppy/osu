// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Profile.Sections
{
    /// <summary>
    /// Display artist/title/mapper information, commonly used as the left portion of a profile or score display row.
    /// </summary>
    public abstract class BeatmapMetadataContainer : OsuHoverContainer
    {
        private readonly BeatmapInfo beatmapInfo;

        protected BeatmapMetadataContainer(BeatmapInfo beatmapInfo)
            : base(HoverSampleSet.Submit)
        {
            this.beatmapInfo = beatmapInfo;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay beatmapSetOverlay)
        {
            Action = () =>
            {
                if (beatmapInfo.OnlineBeatmapID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmap(beatmapInfo.OnlineBeatmapID.Value);
                else if (beatmapInfo.BeatmapSet?.OnlineBeatmapSetID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmapInfo.BeatmapSet.OnlineBeatmapSetID.Value);
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = CreateText(beatmapInfo),
            };
        }

        protected abstract Drawable[] CreateText(BeatmapInfo beatmapInfo);
    }
}
