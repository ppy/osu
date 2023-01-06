// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class CommentPostRequest : APIRequest<CommentBundle>
    {
        public readonly CommentableType Commentable;
        public readonly long CommentableId;
        public readonly string Message;
        public readonly long? ReplyTo;

        public CommentPostRequest(CommentableType commentable, long commentableId, string message, long? replyTo = null)
        {
            Commentable = commentable;
            CommentableId = commentableId;
            Message = message;
            ReplyTo = replyTo;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;

            req.AddParameter(@"comment[commentable_type]", Commentable.ToString().ToLowerInvariant());
            req.AddParameter(@"comment[commentable_id]", $"{CommentableId}");
            req.AddParameter(@"comment[message]", Message);
            if (ReplyTo.HasValue)
                req.AddParameter(@"comment[parent_id]", $"{ReplyTo}");

            return req;
        }

        protected override string Target => "comments";
    }
}
