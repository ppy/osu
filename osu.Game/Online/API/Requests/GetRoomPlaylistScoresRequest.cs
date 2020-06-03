// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests
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
        public List<RoomScore> Scores { get; set; }
    }
}
