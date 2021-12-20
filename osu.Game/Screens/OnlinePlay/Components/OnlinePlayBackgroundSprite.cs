// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class OnlinePlayBackgroundSprite : OnlinePlayComposite
    {
        protected readonly BeatmapSetCoverType BeatmapSetCoverType;
        private UpdateableBeatmapBackgroundSprite sprite;

        public OnlinePlayBackgroundSprite(BeatmapSetCoverType beatmapSetCoverType = BeatmapSetCoverType.Cover)
        {
            BeatmapSetCoverType = beatmapSetCoverType;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = sprite = CreateBackgroundSprite();

            Playlist.CollectionChanged += (_, __) => updateBeatmap();

            updateBeatmap();
        }

        private void updateBeatmap()
        {
            sprite.Beatmap.Value = Playlist.GetCurrentItem()?.Beatmap.Value;
        }

        protected virtual UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType) { RelativeSizeAxes = Axes.Both };
    }
}
