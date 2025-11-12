// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public interface IMatchmakingPlaylistItem
    {
        const long ID_RANDOM = -1;

        MultiplayerPlaylistItem PlaylistItem { get; }

        float LayoutPosition { get; }

        long ID => PlaylistItem.ID;
    }

    public record MatchmakingPlaylistItemBeatmap(MultiplayerPlaylistItem PlaylistItem, APIBeatmap Beatmap, Mod[] Mods) : IMatchmakingPlaylistItem
    {
        public float LayoutPosition => (float)PlaylistItem.StarRating;
    }

    public record MatchmakingPlaylistItemRandom : IMatchmakingPlaylistItem
    {
        public MultiplayerPlaylistItem PlaylistItem { get; } = new MultiplayerPlaylistItem { ID = IMatchmakingPlaylistItem.ID_RANDOM };

        public float LayoutPosition => float.MinValue;
    }
}
