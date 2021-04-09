// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Solo
{
    public class CreateSoloScoreRequest : APIRequest<APIScoreToken>
    {
        private readonly int beatmapId;
        private readonly string versionHash;

        public CreateSoloScoreRequest(int beatmapId, string versionHash)
        {
            this.beatmapId = beatmapId;
            this.versionHash = versionHash;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddParameter("version_hash", versionHash);
            return req;
        }

        protected override string Target => $@"beatmaps/{beatmapId}/solo/scores";
    }
}
