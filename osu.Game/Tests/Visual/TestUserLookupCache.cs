// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Tests.Visual
{
    public partial class TestUserLookupCache : UserLookupCache
    {
        /// <summary>
        /// A special user ID which <see cref="ComputeValueAsync"/> would return a <see langword="null"/> <see cref="APIUser"/> for.
        /// As a simulation to what a regular <see cref="UserLookupCache"/> would return in the case of failing to fetch the user.
        /// </summary>
        public const int UNRESOLVED_USER_ID = -1;

        protected override Task<APIUser?> ComputeValueAsync(int lookup, CancellationToken token = default)
        {
            if (lookup == UNRESOLVED_USER_ID)
                return Task.FromResult<APIUser?>(null);

            return Task.FromResult<APIUser?>(new APIUser
            {
                Id = lookup,
                Username = $"User {lookup}"
            });
        }
    }
}
