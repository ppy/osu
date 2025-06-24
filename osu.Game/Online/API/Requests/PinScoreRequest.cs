// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests
{
    public class PinScoreRequest : APIRequest
    {
        private readonly IScoreInfo score;

        public PinScoreRequest(IScoreInfo score)
        {
            this.score = score;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();
            request.Method = HttpMethod.Put;
            return request;
        }

        protected override string Target => $"score-pins/{score.OnlineID}";
    }
}
