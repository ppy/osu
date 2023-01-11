// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

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

        // TODO: add ruleset

        public UserProfileData(APIUser user)
        {
            User = user;
        }
    }
}
