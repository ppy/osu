// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetCommentRepliesRequest : APIRequest<CommentBundle>
    {
        private readonly long id;

        public GetCommentRepliesRequest(long id)
        {
            this.id = id;
        }

        protected override string Target => $@"comments/{id}";
    }
}
