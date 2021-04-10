// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Solo
{
    public class CreateSoloScoreRequest : APIRequest<APIScoreToken>
    {
        private readonly int beatmapId;
        private readonly int rulesetId;
        private readonly string versionHash;

        public CreateSoloScoreRequest(int beatmapId, int rulesetId, string versionHash)
        {
            this.beatmapId = beatmapId;
            this.rulesetId = rulesetId;
            this.versionHash = versionHash;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddParameter("version_hash", versionHash);
            req.AddParameter("ruleset_id", rulesetId.ToString(CultureInfo.InvariantCulture));
            return req;
        }

        protected override string Target => $@"beatmaps/{beatmapId}/solo/scores";
    }
}
