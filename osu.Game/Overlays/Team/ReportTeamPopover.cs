// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Team
{
    public partial class ReportTeamPopover : ReportPopover<TeamReportReason>
    {
        private readonly APITeam team;

        public ReportTeamPopover(APITeam team)
            : base(ReportStrings.TeamTitle(team.Leader.Username))
        {
            this.team = team;
        }

        protected override APIRequest GetRequest(TeamReportReason reason, string comments) => new TeamReportRequest(team.Id, reason, comments);
    }
}
