// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetRankingsPerformanceRequest : APIRequest<List<APIUser>>
    {
        private readonly RulesetInfo ruleset;
        private readonly int page;

        public GetRankingsPerformanceRequest(RulesetInfo ruleset, int page = 0)
        {
            this.ruleset = ruleset;
            this.page = page;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("page", page.ToString());

            return req;
        }

        protected override string Target => $"rankings/{ruleset.ShortName}/performance";
    }
}
