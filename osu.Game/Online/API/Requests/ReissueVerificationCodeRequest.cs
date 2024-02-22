// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class ReissueVerificationCodeRequest : APIRequest
    {
        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;

            return req;
        }

        protected override string Target => @"session/verify/reissue";
    }
}
