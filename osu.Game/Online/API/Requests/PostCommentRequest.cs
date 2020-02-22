// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using System.Net.Http;

namespace osu.Game.Online.API.Requests
{
    public class PostCommentRequest : APIRequest<CommentBundle>
    {
        private readonly long commentableId;
        private readonly CommentableType type;
        private readonly string message;
        private readonly long? parentId;

        public PostCommentRequest(long commentableId, CommentableType type, string message, long? parentId = null)
        {
            this.commentableId = commentableId;
            this.type = type;
            this.message = message;
            this.parentId = parentId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;

            req.AddParameter("comment[commentable_id]", commentableId.ToString());
            req.AddParameter("comment[commentable_type]", type.ToString().Underscore().ToLowerInvariant());
            req.AddParameter("comment[message]", message);

            if (parentId.HasValue)
                req.AddParameter("comment[parent_id]", parentId.Value.ToString());

            return req;
        }

        protected override string Target => "comments";
    }
}
