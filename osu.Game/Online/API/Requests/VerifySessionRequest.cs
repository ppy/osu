// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class VerifySessionRequest : APIRequest
    {
        public readonly string VerificationKey;

        public VerifySessionRequest(string verificationKey)
        {
            VerificationKey = verificationKey;

            Failure += _ =>
            {
                string? response = WebRequest?.GetResponseString();
                if (string.IsNullOrEmpty(response))
                    return;

                var responseObject = JsonConvert.DeserializeObject<VerificationFailureResponse>(response);
                RequiredVerificationMethod = responseObject?.RequiredSessionVerificationMethod;
            };
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.AddParameter(@"verification_key", VerificationKey);

            return req;
        }

        protected override string Target => @"session/verify";

        public SessionVerificationMethod? RequiredVerificationMethod { get; internal set; }

        private class VerificationFailureResponse
        {
            [JsonProperty("method")]
            public SessionVerificationMethod RequiredSessionVerificationMethod { get; set; }
        }
    }
}
