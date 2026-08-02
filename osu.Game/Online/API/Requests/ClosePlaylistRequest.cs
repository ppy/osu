// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class ClosePlaylistRequest : APIRequest
    {
        private readonly long roomId;

        public ClosePlaylistRequest(long roomId)
        {
            this.roomId = roomId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();
            request.Method = HttpMethod.Delete;
            return request;
        }

        protected override string Target => $@"rooms/{roomId}";
    }
}
