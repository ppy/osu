// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private readonly long? userId;
        public readonly RulesetInfo Ruleset;

        public GetUserRequest(long? userId = null, RulesetInfo ruleset = null)
        {
            this.userId = userId;
            Ruleset = ruleset;
        }

        protected override string Target => userId.HasValue ? $@"users/{userId}/{Ruleset?.ShortName}" : $@"me/{Ruleset?.ShortName}";
    }
}
