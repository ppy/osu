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
        protected override Task<User> ComputeValueAsync(int lookup, CancellationToken token = default) => Task.FromResult(new User
        {
            Id = lookup,
            Username = $"User {lookup}"
        });
    }
}
