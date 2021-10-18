// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Database;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public class TestUserLookupCache : UserLookupCache
    {
        /// <summary>
        /// A special user ID which <see cref="ComputeValueAsync"/> would return a <see langword="null"/> <see cref="User"/> for.
        /// As a simulation to what a regular <see cref="UserLookupCache"/> would return in the case of failing to fetch the user.
        /// </summary>
        public const int NULL_USER_ID = -1;

        protected override Task<User> ComputeValueAsync(int lookup, CancellationToken token = default)
        {
            if (lookup == NULL_USER_ID)
                return Task.FromResult((User)null);

            return Task.FromResult(new User
            {
                Id = lookup,
                Username = $"User {lookup}"
            });
        }
    }
}
