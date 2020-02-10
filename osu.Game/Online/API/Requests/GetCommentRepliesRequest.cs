// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;

namespace osu.Game.Online.API.Requests
{
    public class GetCommentRepliesRequest : APIRequest<CommentBundle>
    {
        private readonly long commentId;
        private readonly CommentableType commentableType;
        private readonly long commentableId;
        private readonly int page;
        private readonly CommentsSortCriteria sort;

        public GetCommentRepliesRequest(long commentId, CommentableType commentableType, long commentableId, CommentsSortCriteria sort, int page)
        {
            this.commentId = commentId;
            this.page = page;
            this.sort = sort;

            // These parameters are necessary to get deleted comments
            this.commentableType = commentableType;
            this.commentableId = commentableId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("parent_id", commentId.ToString());
            req.AddParameter("sort", sort.ToString().ToLowerInvariant());
            req.AddParameter("commentable_type", commentableType.ToString().Underscore().ToLowerInvariant());
            req.AddParameter("commentable_id", commentableId.ToString());
            req.AddParameter("page", page.ToString());

            return req;
        }

        protected override string Target => "comments";
    }
}
