// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Profile.Sections
{
    /// <summary>
    /// Display artist/title/mapper information, commonly used as the left portion of a profile or score display row.
    /// </summary>
    public abstract partial class BeatmapMetadataContainer : OsuHoverContainer
    {
        private readonly IBeatmapInfo beatmapInfo;

        protected BeatmapMetadataContainer(IBeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapSetOverlay? beatmapSetOverlay)
        {
            Action = () =>
            {
                beatmapSetOverlay?.FetchAndShowBeatmap(beatmapInfo.OnlineID);
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = CreateText(beatmapInfo),
            };
        }

        protected abstract Drawable[] CreateText(IBeatmapInfo beatmapInfo);
    }
}
