// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API.Requests;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class LocalResponseContainer : ResponseContainer
    {
        public readonly BindableBool Expanded = new BindableBool();

        private readonly Comment comment;

        public LocalResponseContainer(Comment comment)
        {
            this.comment = comment;

            Height = 60;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(expanded =>
            {
                if (expanded.NewValue)
                    Show();
                else
                {
                    Hide();
                    TextBox.Current.Value = string.Empty;
                }
            }, true);
        }

        protected override Button[] AddButtons() => new Button[]
        {
            new CancelButton
            {
                Expanded = { BindTarget = Expanded }
            }
        };

        protected override PostButton CreatePostButton() => new ReplyButton
        {
            ClickAction = OnAction,
            Expanded = { BindTarget = Expanded },
            Text = { BindTarget = Text }
        };

        protected override PostCommentRequest CreateRequest() =>
            new PostCommentRequest(comment.CommentableId, comment.CommentableType, TextBox.Current.Value, comment.Id);

        protected override Comment OnSuccess(CommentBundle response)
        {
            Comment newReply = response.Comments.First();
            newReply.ParentComment = comment;
            return newReply;
        }

        private class CancelButton : Button
        {
            public readonly BindableBool Expanded = new BindableBool();

            public CancelButton()
                : base(@"Cancel")
            {
                Action = () => Expanded.Value = false;
            }

            protected override void OnLoadStarted() => IsLoading = false;
        }

        private class ReplyButton : PostButton
        {
            public readonly BindableBool Expanded = new BindableBool();

            public ReplyButton()
                : base(@"Reply")
            {
            }

            protected override void OnLoadFinished()
            {
                base.OnLoadFinished();
                Expanded.Value = false;
            }
        }
    }
}
