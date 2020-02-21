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
using JetBrains.Annotations;

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

            appendComments(commentBundle);
        }

        private DrawableComment getDrawableComment(Comment comment)
        {
            if (commentDictionary.TryGetValue(comment.Id, out var existing))
                return existing;

            return commentDictionary[comment.Id] = new DrawableComment(comment)
            {
                ShowDeleted = { BindTarget = ShowDeleted },
                Sort = { BindTarget = Sort },
                RepliesRequested = onCommentRepliesRequested
            };
        }

        private void onCommentRepliesRequested(DrawableComment drawableComment, int page)
        {
            var request = new GetCommentsRequest(CommentableId.Value, Type.Value, Sort.Value, page, drawableComment.Comment.Id);

            request.Success += response => Schedule(() => appendComments(response));

            api.PerformAsync(request);
        }

        private readonly Dictionary<long, DrawableComment> commentDictionary = new Dictionary<long, DrawableComment>();

        /// <summary>
        /// Appends retrieved comments to the subtree rooted of comments in this page.
        /// </summary>
        /// <param name="bundle">The bundle of comments to add.</param>
        private void appendComments([NotNull] CommentBundle bundle)
        {
            var orphaned = new List<Comment>();

            foreach (var topLevel in bundle.Comments)
                addNewComment(topLevel);

            foreach (var child in bundle.IncludedComments)
            {
                // Included comments can contain the parent comment, which already exists in the hierarchy.
                if (commentDictionary.ContainsKey(child.Id))
                    continue;

                addNewComment(child);
            }

            // Comments whose parents were seen later than themselves can now be added.
            foreach (var o in orphaned)
                addNewComment(o);

            void addNewComment(Comment comment)
            {
                var drawableComment = getDrawableComment(comment);

                if (comment.ParentId == null)
                {
                    // Comments that have no parent are added as top-level comments to the flow.
                    flow.Add(drawableComment);
                }
                else if (commentDictionary.TryGetValue(comment.ParentId.Value, out var parentDrawable))
                {
                    // The comment's parent has already been seen, so the parent<-> child links can be added.
                    comment.ParentComment = parentDrawable.Comment;
                    parentDrawable.Replies.Add(drawableComment);
                }
                else
                {
                    // The comment's parent has not been seen yet, so keep it orphaned for the time being. This can occur if the comments arrive out of order.
                    // Since this comment has now been seen, any further children can be added to it without being orphaned themselves.
                    orphaned.Add(comment);
                }
            }
        }

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
                        Text = @"暂无评论"
                    }
                });
            }
        }
    }
}
