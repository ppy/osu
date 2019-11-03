// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using Humanizer;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;

namespace osu.Game.Online.API.Requests
{
    public class GetCommentsRequest : APIRequest<CommentBundle>
    {
        private readonly CommentBundleParameters parameters;
        private readonly int page;
        private readonly CommentsSortCriteria sort;

        public GetCommentsRequest(CommentBundleParameters parameters, CommentsSortCriteria sort = CommentsSortCriteria.New, int page = 1)
        {
            this.parameters = parameters;
            this.sort = sort;
            this.page = page;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (!parameters.IsEmpty)
            {
                req.AddParameter("commentable_type", parameters.Type.Value.ToString().Underscore().ToLowerInvariant());
                req.AddParameter("commentable_id", parameters.Id.Value.ToString());
            }
            req.AddParameter("sort", sort.ToString().ToLowerInvariant());
            req.AddParameter("page", page.ToString());

            return req;
        }

        protected override string Target => "comments";
    }

    public enum CommentableType
    {
        Build,
        Beatmapset,
        NewsPost
    }
}
