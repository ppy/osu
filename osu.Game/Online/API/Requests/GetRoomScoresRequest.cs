// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetRoomScoresRequest : APIRequest<List<APIRoomScoreInfo>>
    {
        private readonly int roomId;

        public GetRoomScoresRequest(int roomId)
        {
            this.roomId = roomId;
        }

        protected override string Target => $@"rooms/{roomId}/leaderboard";
    }
}
