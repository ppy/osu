// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRankingsRequest : APIRequest<List<APIUserRankings>>
    {
        private readonly RulesetInfo ruleset;
        private readonly int page;
        private readonly string country;
        private readonly UserRankingsType type;

        public GetUserRankingsRequest(RulesetInfo ruleset, UserRankingsType type = UserRankingsType.Performance, int page = 0, string country = null)
        {
            this.type = type;
            this.ruleset = ruleset;
            this.page = page;
            this.country = country;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("page", page.ToString());

            if (country != null)
                req.AddParameter("country", country);

            return req;
        }

        protected override string Target => $"rankings/{ruleset.ShortName}/{type.ToString().ToLowerInvariant()}";
    }

    public enum UserRankingsType
    {
        Performance,
        Score
    }
}
