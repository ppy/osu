// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Online.Solo
{
    public class SubmitSoloScoreRequest : APIRequest<MultiplayerScore>
    {
        private readonly long scoreId;

        private readonly int beatmapId;

        private readonly SubmittableScore score;

        public SubmitSoloScoreRequest(int beatmapId, long scoreId, ScoreInfo scoreInfo)
        {
            this.beatmapId = beatmapId;
            this.scoreId = scoreId;
            score = new SubmittableScore(scoreInfo);
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.ContentType = "application/json";
            req.Method = HttpMethod.Put;

            req.AddRaw(JsonConvert.SerializeObject(score, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return req;
        }

        protected override string Target => $@"beatmaps/{beatmapId}/solo/scores/{scoreId}";
    }
}
