// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Profile
{
    /// <summary>
    /// Contains data about a profile presented on the <see cref="UserProfileOverlay"/>.
    /// </summary>
    public class UserProfileData
    {
        /// <summary>
        /// The user whose profile is being presented.
        /// </summary>
        public APIUser User { get; }

        /// <summary>
        /// The ruleset that the user profile is being shown with.
        /// </summary>
        public RulesetInfo Ruleset { get; }

        public UserProfileData(APIUser user, RulesetInfo ruleset)
        {
            User = user;
            Ruleset = ruleset;
        }
    }
}
