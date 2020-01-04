// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private readonly long? userId;
        private readonly RulesetInfo ruleset;

        public GetUserRequest(long? userId = null, RulesetInfo ruleset = null)
        {
            this.userId = userId;
            this.ruleset = ruleset;
        }

        protected override string Target => userId.HasValue ? $@"users/{userId}/{ruleset?.ShortName}" : $@"me/{ruleset?.ShortName}";
    }
}
