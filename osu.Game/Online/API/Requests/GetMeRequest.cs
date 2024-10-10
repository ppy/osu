// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetMeRequest : APIRequest<APIMe>
    {
        public readonly IRulesetInfo? Ruleset;

        /// <summary>
        /// Gets the currently logged-in user.
        /// </summary>
        /// <param name="ruleset">The ruleset to get the user's info for.</param>
        public GetMeRequest(IRulesetInfo? ruleset = null)
        {
            Ruleset = ruleset;
        }

        protected override string Target => $@"me/{Ruleset?.ShortName}";
    }
}
