// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetTeamRequest : APIRequest<APITeam>
    {
        public readonly string Lookup;
        public readonly IRulesetInfo? Ruleset;

        public GetTeamRequest(long? teamId = null, IRulesetInfo? ruleset = null)
        {
            Lookup = teamId.ToString()!;
            Ruleset = ruleset;
        }

        public GetTeamRequest(string shortName, IRulesetInfo? ruleset = null)
        {
            Lookup = $"@{shortName}";
            Ruleset = ruleset;
        }

        protected override string Target => $@"teams/{Lookup}/{Ruleset?.ShortName}";
    }
}
