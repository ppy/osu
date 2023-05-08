// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public partial class ReplyCommentEditor : CancellableCommentEditor
    {
        [Resolved]
        private CommentsContainer commentsContainer { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Comment parentComment;

        public Action<DrawableComment[]>? OnPost;

        protected override LocalisableString FooterText => default;
        protected override LocalisableString CommitButtonText => CommonStrings.ButtonsReply;
        protected override LocalisableString TextBoxPlaceholder => CommentsStrings.PlaceholderReply;

        public ReplyCommentEditor(Comment parent)
        {
            parentComment = parent;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            GetContainingInputManager().ChangeFocus(TextBox);
        }

        protected override void OnCommit(string text)
        {
            ShowLoadingSpinner = true;
            CommentPostRequest req = new CommentPostRequest(commentsContainer.Type.Value, commentsContainer.Id.Value, text, parentComment.Id);
            req.Failure += e => Schedule(() =>
            {
                ShowLoadingSpinner = false;
                Logger.Error(e, "Posting reply comment failed.");
            });
            req.Success += cb => Schedule(processPostedComments, cb);
            api.Queue(req);
        }

        private void processPostedComments(CommentBundle cb)
        {
            foreach (var comment in cb.Comments)
                comment.ParentComment = parentComment;

            var drawables = cb.Comments.Select(commentsContainer.GetDrawableComment).ToArray();
            OnPost?.Invoke(drawables);

            OnCancel!.Invoke();
        }
    }
}
