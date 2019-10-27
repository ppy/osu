// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API.Requests;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class GlobalResponseContainer : ResponseContainer
    {
        private CommentableType? type;
        private long? id;

        public void SetParameters(CommentableType type, long id)
        {
            this.type = type;
            this.id = id;
        }

        public GlobalResponseContainer()
        {
            Height = 70;
        }

        protected override PostCommentRequest CreateRequest() =>
            !type.HasValue || !id.HasValue ? null : new PostCommentRequest(id.Value, type.Value, TextBox.Current.Value);

        protected override Comment OnSuccess(CommentBundle response)
        {
            Comment newReply = response.Comments.First();
            return newReply;
        }
    }
}
