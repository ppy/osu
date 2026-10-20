// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile
{
    public partial class ReportUserDialog : ReportDialog<UserReportReason>
    {
        private readonly APIUser user;

        public ReportUserDialog(APIUser user)
            : base(ReportStrings.UserTitle(user.Username))
        {
            this.user = user;
        }

        protected override APIRequest CreateRequest(UserReportReason reason, string comments) => new UserReportRequest(user.Id, reason, comments);
    }
}
