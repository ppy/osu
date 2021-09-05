// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private readonly string userIdentifier;
        public readonly RulesetInfo Ruleset;


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
            this.userIdentifier = userId.ToString();
            Ruleset = ruleset;
        }

        /// <summary>
        /// Gets a user from their username.
        /// </summary>
        /// <param name="username">The user to get.</param>
        /// <param name="ruleset">The ruleset to get the user's info for.</param>
        public GetUserRequest(string username = null, RulesetInfo ruleset = null)
        {
            this.userIdentifier = username;
            Ruleset = ruleset;
        }

        protected override string Target => userIdentifier != null ? $@"users/{userIdentifier}/{Ruleset?.ShortName}" : $@"me/{Ruleset?.ShortName}";
    }
}
