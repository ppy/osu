// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile
{
    public partial class ReportUserPopover : ReportPopover<UserReportReason>
    {
        private readonly APIUser user;

        public ReportUserPopover(APIUser user)
            : base(ReportStrings.UserTitle(user.Username))
        {
            this.user = user;
        }

        protected override APIRequest GetRequest(UserReportReason reason, string comments) => new UserReportRequest(user.Id, reason, comments);
    }
}
