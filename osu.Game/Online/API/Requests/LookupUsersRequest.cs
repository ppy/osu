// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    /// <summary>
    /// Looks up users with the given <see cref="UserIds"/>.
    /// In comparison to <see cref="GetUsersRequest"/>, the response here does not contain <see cref="APIUser.RulesetsStatistics"/>,
    /// but in exchange is subject to less stringent rate limiting, making it suitable for mass user listings.
    ///
    /// Providing a ruleset ID will give `global_rank`s in the response.
    /// </summary>
    public class LookupUsersRequest : APIRequest<GetUsersResponse>
    {
        public readonly int[] UserIds;

        public readonly int? RulesetId;

        private const int max_ids_per_request = 50;

        public LookupUsersRequest(int[] userIds, int? rulesetId = null)
        {
            if (userIds.Length > max_ids_per_request)
                throw new ArgumentException($"{nameof(LookupUsersRequest)} calls only support up to {max_ids_per_request} IDs at once");

            UserIds = userIds;
            RulesetId = rulesetId;
        }

        protected override string Target => @"users/lookup/?ids[]=" + string.Join(@"&ids[]=", UserIds) + (RulesetId != null ? "&ruleset_id=" + RulesetId : "");
    }
}
