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
    /// Display artist/title/mapper information, commonly used as the left portion of a profile or score display row (see <see cref="DrawableProfileRow"/>).
    /// </summary>
    public abstract class BeatmapMetadataContainer : OsuHoverContainer
    {
        private readonly BeatmapInfo beatmap;

        protected BeatmapMetadataContainer(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay beatmapSetOverlay)
        {
            Action = () =>
            {
                if (beatmap.OnlineBeatmapID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmap(beatmap.OnlineBeatmapID.Value);
                else if (beatmap.BeatmapSet?.OnlineBeatmapSetID != null)
                    beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmap.BeatmapSet.OnlineBeatmapSetID.Value);
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = CreateText(beatmap),
            };
        }

        protected abstract Drawable[] CreateText(BeatmapInfo beatmap);
    }
}
