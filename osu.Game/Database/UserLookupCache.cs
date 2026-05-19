// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Database
{
    public partial class UserLookupCache : OnlineLookupCache<int, APIUser, LookupUsersRequest>
    {
        /// <summary>
        /// Perform an API lookup on the specified user, populating a <see cref="APIUser"/> model.
        /// </summary>
        /// <param name="userId">The user to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated user, or null if the user does not exist or the request could not be satisfied.</returns>
        public Task<APIUser?> GetUserAsync(int userId, CancellationToken token = default) => LookupAsync(userId, token);

        /// <summary>
        /// Perform an API lookup on the specified users, populating a <see cref="APIUser"/> model.
        /// </summary>
        /// <param name="userIds">The users to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated users. May include null results for failed retrievals.</returns>
        public Task<APIUser?[]> GetUsersAsync(int[] userIds, CancellationToken token = default) => LookupAsync(userIds, token);

        /// <summary>
        /// Invalidate previously looked up users with <see cref="GetuserAsync"/> or <see cref="GetUsersAsync"/>. Used to clear only a part of the cache, instead of the entire package.
        /// </summary>
        /// <param name="userIds"></param>
        public void InvalidateUsers(IEnumerable<int> userIds)
        {
            var idSet = userIds.ToHashSet();
            Invalidate(id => idSet.Contains(id));
        }

        protected override LookupUsersRequest CreateRequest(IEnumerable<int> ids) => new LookupUsersRequest(ids.ToArray());

        protected override IEnumerable<APIUser>? RetrieveResults(LookupUsersRequest request) => request.Response?.Users;
    }
}
