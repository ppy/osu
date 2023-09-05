// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;

namespace osu.Game.Online.Rooms
{
    public abstract class SubmitScoreRequest<TScore> : APIRequest<TScore>
        where TScore : class, IAPISubmittedScore
    {
        public readonly SoloScoreInfo Score;

        protected readonly long ScoreTokenId;

        protected SubmitScoreRequest(ScoreInfo scoreInfo, long scoreTokenId)
        {
            Score = SoloScoreInfo.ForSubmission(scoreInfo);
            ScoreTokenId = scoreTokenId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.ContentType = "application/json";
            req.Method = HttpMethod.Put;
            req.Timeout = 30000;

            req.AddRaw(JsonConvert.SerializeObject(Score, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return req;
        }
    }
}
