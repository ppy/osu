// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API;

namespace osu.Game.Online.Multiplayer
{
    public class GetRoomPlaylistScoresRequest : APIRequest<RoomPlaylistScores>
    {
        private readonly int roomId;
        private readonly int playlistItemId;

        public GetRoomPlaylistScoresRequest(int roomId, int playlistItemId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores";
    }

    public class RoomPlaylistScores
    {
        [JsonProperty("scores")]
        public List<MultiplayerScore> Scores { get; set; }
    }
}
