// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Team
{
    /// <summary>
    /// Contains data about a profile presented on the <see cref="TeamProfileOverlay"/>.
    /// </summary>
    public class TeamProfileData
    {
        /// <summary>
        /// The team whose profile is being presented.
        /// </summary>
        public APITeam Team { get; }

        /// <summary>
        /// The ruleset that the team profile is being shown with.
        /// </summary>
        public RulesetInfo Ruleset { get; }

        public TeamProfileData(APITeam team, RulesetInfo ruleset)
        {
            Team = team;
            Ruleset = ruleset;
        }
    }
}
