// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetRoomPlaylistScoresRequest : APIRequest<List<RoomScore>>
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
}
