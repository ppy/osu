// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetMatchmakingRankingRequest : APIRequest<GetMatchmakingRankingResponse>
    {
        public readonly RulesetInfo Ruleset;
        public readonly APIMatchmakingPool Pool;

        public GetMatchmakingRankingRequest(RulesetInfo ruleset, APIMatchmakingPool pool)
        {
            Ruleset = ruleset;
            Pool = pool;
        }

        protected override string Target => $"rankings/ranked-play/{Ruleset.ShortName}/{Pool.Id}";
    }
}
