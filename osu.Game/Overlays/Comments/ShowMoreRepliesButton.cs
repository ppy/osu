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
    public class ShowMoreRepliesButton : LoadingButton
    {
        public readonly Bindable<List<Comment>> ChildComments = new Bindable<List<Comment>>();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Comment comment;
        private SpriteText text;
        private ShowMoreCommentRepliesRequest request;
        private int currentPage;

        public ShowMoreRepliesButton(Comment comment)
        {
            this.comment = comment;

            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding { Vertical = 10, Left = 80 };
            LoadingAnimationSize = new Vector2(8);

            Action = onAction;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.Blue;
            HoverColour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ChildComments.BindValueChanged(children =>
            {
                Alpha = (!children.NewValue.Any() && comment.RepliesCount > 0) || children.NewValue.Count == comment.RepliesCount ? 0 : 1;
            }, true);
        }

        protected override Container CreateBackground() => new Container
        {
            AutoSizeAxes = Axes.Both
        };

        protected override Drawable CreateContent() => text = new SpriteText
        {
            AlwaysPresent = true,
            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
            Text = @"Show More"
        };

        private void onAction()
        {
            request = new ShowMoreCommentRepliesRequest(comment.Id, Sort.Value, ++currentPage);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(CommentBundle response)
        {
            var children = ChildComments.Value.ToList();
            List<Comment> receivedReplies = new List<Comment>();
            response.Comments.ForEach(c =>
            {
                if (c.ParentId == comment.Id)
                    receivedReplies.Add(c);
            });
            receivedReplies.ForEach(r =>
            {
                if (!children.Contains(r))
                    children.Add(r);
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
