// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public record MatchmakingPlaylistItem(MultiplayerPlaylistItem PlaylistItem, APIBeatmap Beatmap, Mod[] Mods)
    {
        public long ID => PlaylistItem.ID;
    }
}
