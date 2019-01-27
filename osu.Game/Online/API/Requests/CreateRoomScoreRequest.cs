// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class CreateRoomScoreRequest : APIRequest<APIScoreToken>
    {
        private readonly int roomId;
        private readonly int playlistItemId;

        public CreateRoomScoreRequest(int roomId, int playlistItemId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores";
    }
}
