// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class PlaylistItemBackground : Background
    {
        public readonly IBeatmapInfo? Beatmap;

        public PlaylistItemBackground(PlaylistItem? playlistItem)
        {
            Beatmap = playlistItem?.Beatmap;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, LargeTextureStore textures)
        {
            Texture? texture = null;

            // prefer online cover where available.
            if (Beatmap?.BeatmapSet is IBeatmapSetOnlineInfo online)
                texture = textures.Get(online.Covers.Cover);

            Sprite.Texture = texture ?? beatmaps.DefaultBeatmap.GetBackground();
        }

        public override bool Equals(Background? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((PlaylistItemBackground)other).Beatmap == Beatmap;
        }
    }
}
