// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;

namespace osu.Game.Online.API.Requests
{
    public class GetCommentRepliesRequest : APIRequest<CommentBundle>
    {
        private readonly long id;
        private readonly int page;
        private readonly CommentsSortCriteria sort;

        public GetCommentRepliesRequest(long id, CommentsSortCriteria sort, int page = 1)
        {
            this.id = id;
            this.page = page;
            this.sort = sort;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("parent_id", id.ToString());
            req.AddParameter("sort", sort.ToString().ToLowerInvariant());
            req.AddParameter("page", page.ToString());

            return req;
        }

        protected override string Target => "comments";
    }
}
