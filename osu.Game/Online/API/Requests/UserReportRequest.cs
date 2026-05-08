// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Overlays.Profile;

namespace osu.Game.Online.API.Requests
{
    public class UserReportRequest : APIRequest
    {
        public readonly int UserID;
        public readonly UserReportReason Reason;
        public readonly string Comment;

        public UserReportRequest(int userID, UserReportReason reason, string comment)
        {
            UserID = userID;
            Reason = reason;
            Comment = comment;
        }

        protected override WebRequest CreateWebRequest()
        {
            var request = base.CreateWebRequest();
            request.Method = HttpMethod.Post;

            request.AddParameter(@"reportable_type", @"user");
            request.AddParameter(@"reportable_id", $"{UserID}");
            request.AddParameter(@"reason", Reason.ToString());
            request.AddParameter(@"comments", Comment);

            return request;
        }

        protected override string Target => @"reports";
    }
}
