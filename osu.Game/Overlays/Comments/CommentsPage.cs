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
        private FillFlowContainer flow;

        public CommentsPage(CommentBundle commentBundle)
        {
            this.commentBundle = commentBundle;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
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

            createBaseTree(commentBundle.Comments);
        }

        private void createBaseTree(List<Comment> comments)
        {
            var nodeDictionary = new Dictionary<long, Comment>();
            var topLevelNodes = new List<Comment>();
            var orphanedNodes = new List<Comment>();

            foreach (var comment in comments)
            {
                nodeDictionary.Add(comment.Id, comment);

                if (comment.IsTopLevel)
                    topLevelNodes.Add(comment);

                var orphanedNodesCopy = new List<Comment>(orphanedNodes);

                foreach (var orphan in orphanedNodesCopy)
                {
                    if (orphan.ParentId == comment.Id)
                    {
                        orphan.ParentComment = comment;
                        comment.ChildComments.Add(orphan);
                        orphanedNodes.Remove(orphan);
                    }
                }

                // No need to find parent for top-level comment
                if (comment.IsTopLevel)
                    continue;

                if (nodeDictionary.ContainsKey(comment.ParentId.Value))
                {
                    comment.ParentComment = nodeDictionary[comment.ParentId.Value];
                    nodeDictionary[comment.ParentId.Value].ChildComments.Add(comment);
                }
                else
                    orphanedNodes.Add(comment);
            }

            foreach (var comment in topLevelNodes)
                flow.Add(createCommentWithReplies(comment));
        }

        private DrawableComment createCommentWithReplies(Comment comment)
        {
            var drawableComment = createDrawableComment(comment);

            var replies = comment.ChildComments;

            if (replies.Any())
                drawableComment.InitialReplies.AddRange(replies.Select(createCommentWithReplies));

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
                if (!drawableComment.LoadedReplies.ContainsKey(c.Id))
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
