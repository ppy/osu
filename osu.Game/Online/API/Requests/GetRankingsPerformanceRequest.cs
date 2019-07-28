// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetRankingsPerformanceRequest : PaginatedAPIRequest<List<APIPerformanceRankings>>
    {
        private readonly RulesetInfo ruleset;

        public GetRankingsPerformanceRequest(RulesetInfo ruleset, int page = 0)
            : base(page, 50)
        {
            this.ruleset = ruleset;
        }

        protected override string Target => $"rankings/{ruleset.ShortName}/performance";
    }
}
