// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Comments
{
    public class OnlineCommentsContainer : CommentsContainer
    {
        private GetCommentsRequest request;
        private int currentPage;
        private CommentBundleParameters parameters;

        public void ShowComments(CommentableType type, long id)
        {
            parameters = new CommentBundleParameters(type, id);
            Sort.TriggerChange();
        }

        protected override void OnSortChanged(ValueChangedEvent<CommentsSortCriteria> sort)
        {
            if (parameters == null)
                return;

            OnLoadStarted();
            OnShowMoreAction();
        }

        protected override void OnUserChanged(ValueChangedEvent<User> user) => Sort.TriggerChange();

        protected override void OnLoadStarted()
        {
            request?.Cancel();
            currentPage = 0;
            base.OnLoadStarted();
        }

        protected override void OnShowMoreAction()
        {
            request = new GetCommentsRequest(parameters, Sort.Value, ++currentPage);
            request.Success += response =>
            {
                if (currentPage == 1)
                    ResetComments(response);
                else
                    AddComments(response, false);
            };
            Task.Run(() => request.Perform(API));
        }

        protected override DrawableComment CreateDrawableComment(Comment comment) => new OnlineDrawableComment(comment, null)
        {
            ShowDeleted = { BindTarget = ShowDeleted },
            OnDeleted = OnCommentDeleted
        };

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }

        private class OnlineDrawableComment : DrawableComment
        {
            public OnlineDrawableComment(Comment comment, OnlineDrawableComment drawableParent)
                : base(comment, drawableParent)
            {
            }

            protected override DrawableComment CreateDrawableReply(Comment comment) => new OnlineDrawableComment(comment, this)
            {
                ShowDeleted = { BindTarget = ShowDeleted },
                OnDeleted = OnReplyDeleted
            };

            protected override DeleteCommentButton CreateDeleteButton() => new OnlineDeleteCommentButton(Comment)
            {
                IsDeleted = { BindTarget = IsDeleted }
            };
        }
    }
}
