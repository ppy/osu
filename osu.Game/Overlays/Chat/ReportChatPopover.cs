// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Chat
{
    public partial class ReportChatPopover : ReportPopover<ChatReportReason>
    {
        public ReportChatPopover(APIUser? user)
            : base(ReportStrings.UserTitle(user?.Username ?? @"Someone"))
        {
        }

        protected override bool CheckCanSubmitEmptyComment(ChatReportReason reason)
        {
            return reason != ChatReportReason.Other;
        }
    }
}
