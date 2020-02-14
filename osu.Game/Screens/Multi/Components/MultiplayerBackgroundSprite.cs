// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Multi.Components
{
    public class MultiplayerBackgroundSprite : MultiplayerComposite
    {
        private readonly BeatmapSetCoverType beatmapSetCoverType;
        private UpdateableBeatmapBackgroundSprite sprite;

        public MultiplayerBackgroundSprite(BeatmapSetCoverType beatmapSetCoverType = BeatmapSetCoverType.Cover)
        {
            this.beatmapSetCoverType = beatmapSetCoverType;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = sprite = CreateBackgroundSprite();

            Playlist.ItemsAdded += _ => updateBeatmap();
            Playlist.ItemsRemoved += _ => updateBeatmap();

            updateBeatmap();
        }

        private void updateBeatmap()
        {
            sprite.Beatmap.Value = Playlist.FirstOrDefault()?.Beatmap.Value;
        }

        protected virtual UpdateableBeatmapBackgroundSprite CreateBackgroundSprite() => new UpdateableBeatmapBackgroundSprite(beatmapSetCoverType) { RelativeSizeAxes = Axes.Both };
    }
}
