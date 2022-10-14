// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Overlays.Comments;

namespace osu.Game.Online.API.Requests
{
    public class CommentReportRequest : APIRequest
    {
        public readonly long CommentID;
        public readonly CommentReportReason Reason;
        public readonly string? Info;

        public CommentReportRequest(long commentID, CommentReportReason reason, string? info)
        {
            CommentID = commentID;
            Reason = reason;
            Info = info;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;

            req.AddParameter(@"reportable_type", @"comment");
            req.AddParameter(@"reportable_id", $"{CommentID}");
            req.AddParameter(@"reason", Reason.ToString());
            if (!string.IsNullOrWhiteSpace(Info))
                req.AddParameter(@"comments", Info);

            return req;
        }

        protected override string Target => @"reports";
    }
}
