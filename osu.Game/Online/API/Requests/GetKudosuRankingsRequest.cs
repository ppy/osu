// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public class GetKudosuRankingsRequest : APIRequest<GetKudosuRankingsResponse>
    {
        private readonly int page;

        public GetKudosuRankingsRequest(int page = 1)
        {
            this.page = page;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter(@"page", page.ToString());

            return req;
        }

        protected override string Target => @"rankings/kudosu";
    }
}
