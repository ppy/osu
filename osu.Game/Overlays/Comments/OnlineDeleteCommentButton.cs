// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Comments
{
    public class OnlineDeleteCommentButton : DeleteCommentButton
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly long id;

        private DeleteCommentRequest request;

        public OnlineDeleteCommentButton(Comment comment)
        {
            id = comment.Id;
        }

        protected override void OnAction()
        {
            request = new DeleteCommentRequest(id);
            request.Success += _ => base.OnAction();
            api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
