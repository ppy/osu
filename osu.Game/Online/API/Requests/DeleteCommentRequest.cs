// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using System.Net.Http;

namespace osu.Game.Online.API.Requests
{
    public class DeleteCommentRequest : APIRequest<CommentBundle>
    {
        private readonly long id;

        public DeleteCommentRequest(long id)
        {
            this.id = id;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Delete;
            return req;
        }

        protected override string Target => $"comments/{id}";
    }
}
