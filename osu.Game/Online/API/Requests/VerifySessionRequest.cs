// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class VerifySessionRequest : APIRequest
    {
        public readonly string VerificationKey;

        public VerifySessionRequest(string verificationKey)
        {
            VerificationKey = verificationKey;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.AddParameter(@"verification_key", VerificationKey);

            return req;
        }

        protected override string Target => @"session/verify";
    }
}
