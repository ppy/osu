// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Overlays.Team;

namespace osu.Game.Online.API.Requests
{
    public class TeamReportRequest : APIRequest
    {
        public readonly long TeamID;
        public readonly TeamReportReason Reason;
        public readonly string Comment;

        public TeamReportRequest(long teamID, TeamReportReason reason, string comment)
        {
            TeamID = teamID;
            Reason = reason;
            Comment = comment;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;

            req.AddParameter(@"reportable_type", @"team");
            req.AddParameter(@"reportable_id", $"{TeamID}");
            req.AddParameter(@"reason", Reason.ToString());
            req.AddParameter(@"comments", Comment);

            return req;
        }

        protected override string Target => @"reports";
    }
}
