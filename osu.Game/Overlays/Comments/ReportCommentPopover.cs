// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public partial class ReportCommentPopover : ReportPopover<CommentReportReason>
    {
        private readonly Comment comment;

        public ReportCommentPopover(Comment comment)
            : base(ReportStrings.CommentTitle(comment.User?.Username ?? comment.LegacyName ?? @"Someone"), false)
        {
            this.comment = comment;
        }

        protected override APIRequest GetRequest(CommentReportReason reason, string comments) => new CommentReportRequest(comment.Id, reason, comments);
    }
}
