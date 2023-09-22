// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class CreateRoomScoreRequest : APIRequest<APIScoreToken>
    {
        private readonly long roomId;
        private readonly long playlistItemId;
        private readonly string versionHash;

        public CreateRoomScoreRequest(long roomId, long playlistItemId, string versionHash)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
            this.versionHash = versionHash;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddParameter("version_hash", versionHash);
            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores";
    }
}
