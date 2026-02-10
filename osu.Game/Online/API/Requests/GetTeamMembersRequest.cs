// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetTeamMembersRequest : PaginatedAPIRequest<TeamMembersResponse>
    {
        public readonly int TeamId;

        public GetTeamMembersRequest(int teamId, PaginationParameters pagination)
            : base(pagination)
        {
            TeamId = teamId;
        }

        protected override string Target => $"teams/{TeamId}/members";
    }
}
