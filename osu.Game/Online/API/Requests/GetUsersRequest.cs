// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Online.API.Requests
{
    public class GetUsersRequest : APIRequest<GetUsersResponse>
    {
        private readonly int[] userIds;

        public GetUsersRequest(int[] userIds)
        {
            this.userIds = userIds;
        }

        protected override string Target => $@"users/?{userIds.Select(u => $"ids[]={u}&").Aggregate((a, b) => a + b)}";
    }
}
