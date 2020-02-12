// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using System.Linq;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Overlays.Comments
{
    public class CommentsPage : CompositeDrawable
    {
        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly Bindable<CommentableType> Type = new Bindable<CommentableType>();
        public readonly BindableLong CommentableId = new BindableLong();

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly CommentBundle commentBundle;

        public CommentsPage(CommentBundle commentBundle)
        {
            this.commentBundle = commentBundle;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            FillFlowContainer flow;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                }
            });

            if (!commentBundle.Comments.Any())
            {
                flow.Add(new NoCommentsPlaceholder());
                return;
            }

            commentBundle.Comments.ForEach(c =>
            {
                if (c.IsTopLevel)
                    flow.Add(createCommentWithReplies(c, commentBundle));
            });
        }

        private DrawableComment createCommentWithReplies(Comment comment, CommentBundle commentBundle)
        {
            var drawableComment = createDrawableComment(comment);

            var replies = commentBundle.Comments.Where(c => c.ParentId == comment.Id);

            if (replies.Any())
            {
                replies.ForEach(c => c.ParentComment = comment);
                drawableComment.InitialReplies.AddRange(replies.Select(reply => createCommentWithReplies(reply, commentBundle)));
            }

            return drawableComment;
        }

        private void onCommentRepliesRequested(DrawableComment drawableComment, int page)
        {
            var request = new GetCommentRepliesRequest(drawableComment.Comment.Id, Type.Value, CommentableId.Value, Sort.Value, page);
            request.Success += response => onCommentRepliesReceived(response, drawableComment);
            api.PerformAsync(request);
        }

        private void onCommentRepliesReceived(CommentBundle response, DrawableComment drawableComment)
        {
            var receivedComments = response.Comments;

            var uniqueComments = new List<Comment>();

            // We may receive already loaded comments
            receivedComments.ForEach(c =>
            {
                if (drawableComment.LoadedReplies.All(loadedReply => loadedReply.Id != c.Id))
                    uniqueComments.Add(c);
            });

            uniqueComments.ForEach(c => c.ParentComment = drawableComment.Comment);

            drawableComment.AddReplies(uniqueComments.Select(createDrawableComment));
        }

        private DrawableComment createDrawableComment(Comment comment) => new DrawableComment(comment)
        {
            ShowDeleted = { BindTarget = ShowDeleted },
            Sort = { BindTarget = Sort },
            RepliesRequested = onCommentRepliesRequested
        };

        private class NoCommentsPlaceholder : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Height = 80;
                RelativeSizeAxes = Axes.X;
                AddRangeInternal(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding { Left = 50 },
                        Text = @"No comments yet."
                    }
                });
            }
        }
    }
}
