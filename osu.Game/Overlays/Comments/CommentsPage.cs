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

            appendComments(null, commentBundle);
        }

        private readonly Dictionary<long, DrawableComment> commentDictionary = new Dictionary<long, DrawableComment>();

        /// <summary>
        /// Appends retrieved comments to the subtree rooted at a parenting <see cref="DrawableComment"/>.
        /// </summary>
        /// <param name="parent">The parenting <see cref="DrawableComment"/>.</param>
        /// <param name="bundle">The bundle of comments to add.</param>
        private void appendComments([CanBeNull] DrawableComment parent, [NotNull] CommentBundle bundle)
        {
            var orphaned = new List<Comment>();

            foreach (var topLevel in bundle.Comments)
                add(topLevel);

            foreach (var child in bundle.IncludedComments)
                add(child);

            // Comments whose parents did not previously have corresponding drawables, are now guaranteed that their parents have corresponding drawables.
            foreach (var o in orphaned)
                add(o);

            void add(Comment comment)
            {
                var drawableComment = getDrawableComment(comment);

                if (comment.ParentId == null)
                {
                    // Comment that has no parent is added as a top-level comment to the flow.
                    flow.Add(drawableComment);
                }
                else if (commentDictionary.TryGetValue(comment.ParentId.Value, out var parentDrawable))
                {
                    // The comment's parent already has a corresponding drawable.
                    parentDrawable.Replies.Add(drawableComment);
                }
                else
                {
                    // The comment's parent does not have a corresponding drawable yet, so keep it as orphaned for the time being.
                    // Note that this comment's corresponding drawable has already been created by this point, so future children will be able to be added without being orphaned themselves.
                    orphaned.Add(comment);
                }
            }
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
            var request = new GetCommentRepliesRequest(drawableComment.Comment.Id, Type.Value, CommentableId.Value, Sort.Value, page);

            request.Success += response => Schedule(() => appendComments(drawableComment, response));

            api.PerformAsync(request);
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
                        Text = @"No comments yet."
                    }
                });
            }
        }
    }
}
