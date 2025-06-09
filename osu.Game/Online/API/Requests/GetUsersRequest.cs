// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    /// <summary>
    /// Looks up users with the given <see cref="UserIds"/>.
    /// In comparison to <see cref="LookupUsersRequest"/>, the response here contains <see cref="APIUser.RulesetsStatistics"/>,
    /// but in exchange is subject to more stringent rate limiting.
    /// </summary>
    public class GetUsersRequest : APIRequest<GetUsersResponse>
    {
        public const int MAX_IDS_PER_REQUEST = 50;

        public readonly int[] UserIds;

        public GetUsersRequest(int[] userIds)
        {
            if (userIds.Length > MAX_IDS_PER_REQUEST)
                throw new ArgumentException($"{nameof(GetUsersRequest)} calls only support up to {MAX_IDS_PER_REQUEST} IDs at once");

            UserIds = userIds;
        }

        protected override string Target => "users/?ids[]=" + string.Join("&ids[]=", UserIds);
    }
}
