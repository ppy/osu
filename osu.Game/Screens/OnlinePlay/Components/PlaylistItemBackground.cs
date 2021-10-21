// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class PlaylistItemBackground : Background
    {
        public readonly BeatmapInfo? BeatmapInfo;

        public PlaylistItemBackground(PlaylistItem? playlistItem)
        {
            BeatmapInfo = playlistItem?.Beatmap.Value;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, LargeTextureStore textures)
        {
            Texture? texture = null;

            // prefer online cover where available.
            if (BeatmapInfo?.BeatmapSet?.OnlineInfo?.Covers.Cover != null)
                texture = textures.Get(BeatmapInfo.BeatmapSet.OnlineInfo.Covers.Cover);

            Sprite.Texture = texture ?? beatmaps.DefaultBeatmap.Background;
        }

        public override bool Equals(Background? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((PlaylistItemBackground)other).BeatmapInfo == BeatmapInfo;
        }
    }
}
