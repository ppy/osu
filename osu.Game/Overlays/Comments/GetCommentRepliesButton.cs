// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Game.Online.API;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public abstract class GetCommentRepliesButton : LoadingButton
    {
        public readonly Bindable<List<Comment>> ChildComments = new Bindable<List<Comment>>();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        protected readonly Comment Comment;

        [Resolved]
        private IAPIProvider api { get; set; }

        private SpriteText text;
        private GetCommentRepliesRequest request;
        private int currentPage;

        protected GetCommentRepliesButton(Comment comment)
        {
            Comment = comment;

            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
            Action = onAction;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ChildComments.BindValueChanged(OnChildrenChanged, true);
        }

        protected abstract void OnChildrenChanged(ValueChangedEvent<List<Comment>> children);

        protected override Container CreateBackground() => new Container
        {
            AutoSizeAxes = Axes.Both
        };

        protected override Drawable CreateContent() => text = new SpriteText
        {
            AlwaysPresent = true,
            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
            Text = ButtonText(),
        };

        protected abstract string ButtonText();

        private void onAction()
        {
            request = new GetCommentRepliesRequest(Comment.Id, Sort.Value, ++currentPage);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(CommentBundle response)
        {
            var children = ChildComments.Value.ToList();
            response.Comments.ForEach(c =>
            {
                if (children.All(child => child.Id != c.Id))
                    children.Add(c);
            });
            ChildComments.Value = children;
            IsLoading = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
