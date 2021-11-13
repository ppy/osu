// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<APIUser>
    {
        public readonly string Lookup;
        public readonly RulesetInfo Ruleset;
        private readonly LookupType lookupType;

        /// <summary>
        /// Gets the currently logged-in user.
        /// </summary>
        public GetUserRequest()
        {
        }

        /// <summary>
        /// Gets a user from their ID.
        /// </summary>
        /// <param name="userId">The user to get.</param>
        /// <param name="ruleset">The ruleset to get the user's info for.</param>
        public GetUserRequest(long? userId = null, RulesetInfo ruleset = null)
        {
            Lookup = userId.ToString();
            lookupType = LookupType.Id;
            Ruleset = ruleset;
        }

        /// <summary>
        /// Gets a user from their username.
        /// </summary>
        /// <param name="username">The user to get.</param>
        /// <param name="ruleset">The ruleset to get the user's info for.</param>
        public GetUserRequest(string username = null, RulesetInfo ruleset = null)
        {
            Lookup = username;
            lookupType = LookupType.Username;
            Ruleset = ruleset;
        }

        protected override string Target => Lookup != null ? $@"users/{Lookup}/{Ruleset?.ShortName}?key={lookupType.ToString().ToLower()}" : $@"me/{Ruleset?.ShortName}";

        private enum LookupType
        {
            Id,
            Username
        }
    }
}
