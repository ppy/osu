// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetMatchmakingPoolsRequest : APIRequest<List<APIMatchmakingPool>>
    {
        public readonly RulesetInfo Ruleset;

        public GetMatchmakingPoolsRequest(RulesetInfo ruleset)
        {
            Ruleset = ruleset;
        }

        protected override string Target => @$"rankings/ranked-play/{Ruleset.ShortName}";
    }
}
