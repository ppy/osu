// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Overlays.Chat;

namespace osu.Game.Online.API.Requests
{
    public class ChatReportRequest : APIRequest
    {
        public readonly long? MessageId;
        public readonly ChatReportReason Reason;
        public readonly string Comment;

        public ChatReportRequest(long? id, ChatReportReason reason, string comment)
        {
            MessageId = id;
            Reason = reason;
            Comment = comment;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;

            req.AddParameter(@"reportable_type", @"message");
            req.AddParameter(@"reportable_id", $"{MessageId}");
            req.AddParameter(@"reason", Reason.ToString());
            req.AddParameter(@"comments", Comment);

            return req;
        }

        protected override string Target => @"reports";
    }
}
