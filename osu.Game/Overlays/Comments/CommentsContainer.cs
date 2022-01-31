// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using System.Threading;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Threading;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Game.Graphics.Sprites;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private readonly Bindable<CommentableType> type = new Bindable<CommentableType>();
        private readonly BindableLong id = new BindableLong();

        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        protected readonly IBindable<APIUser> User = new Bindable<APIUser>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetCommentsRequest request;
        private ScheduledDelegate scheduledCommentsLoad;
        private CancellationTokenSource loadCancellation;
        private int currentPage;

        private FillFlowContainer pinnedContent;
        private FillFlowContainer content;
        private DeletedCommentsCounter deletedCommentsCounter;
        private CommentsShowMoreButton moreButton;
        private TotalCommentsCounter commentCounter;

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
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        commentCounter = new TotalCommentsCounter(),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background4,
                                },
                                pinnedContent = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                },
                            },
                        },
                        new CommentsHeader
                        {
                            Sort = { BindTarget = Sort },
                            ShowDeleted = { BindTarget = ShowDeleted }
                        },
                        content = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        },
                        new Container
                        {
                            Name = @"Footer",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Margin = new MarginPadding { Bottom = 20 },
                                    Children = new Drawable[]
                                    {
                                        deletedCommentsCounter = new DeletedCommentsCounter
                                        {
                                            ShowDeleted = { BindTarget = ShowDeleted },
                                            Margin = new MarginPadding
                                            {
                                                Horizontal = 70,
                                                Vertical = 10
                                            }
                                        },
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Child = moreButton = new CommentsShowMoreButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Margin = new MarginPadding
                                                {
                                                    Vertical = 10
                                                },
                                                Action = getComments,
                                                IsLoading = true,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            User.BindTo(api.LocalUser);
        }

        protected override void LoadComplete()
        {
            User.BindValueChanged(_ => refetchComments());
            Sort.BindValueChanged(_ => refetchComments(), true);
            base.LoadComplete();
        }

        /// <param name="type">The type of resource to get comments for.</param>
        /// <param name="id">The id of the resource to get comments for.</param>
        public void ShowComments(CommentableType type, long id)
        {
            this.type.Value = type;
            this.id.Value = id;

            if (!IsLoaded)
                return;

            // only reset when changing ID/type. other refetch ops are generally just changing sort order.
            commentCounter.Current.Value = 0;

            refetchComments();
        }

        private void refetchComments()
        {
            ClearComments();
            getComments();
        }

        private void getComments()
        {
            if (id.Value <= 0)
                return;

            request?.Cancel();
            loadCancellation?.Cancel();
            scheduledCommentsLoad?.Cancel();
            request = new GetCommentsRequest(id.Value, type.Value, Sort.Value, currentPage++, 0);
            request.Success += res => scheduledCommentsLoad = Schedule(() => OnSuccess(res));
            api.PerformAsync(request);
        }

        protected void ClearComments()
        {
            currentPage = 1;
            deletedCommentsCounter.Count.Value = 0;
            moreButton.Show();
            moreButton.IsLoading = true;
            pinnedContent.Clear();
            content.Clear();
            CommentDictionary.Clear();
        }

        protected readonly Dictionary<long, DrawableComment> CommentDictionary = new Dictionary<long, DrawableComment>();

        protected void OnSuccess(CommentBundle response)
        {
            commentCounter.Current.Value = response.Total;

            if (!response.Comments.Any())
            {
                content.Add(new NoCommentsPlaceholder());
                moreButton.Hide();
                return;
            }

            AppendComments(response);
        }

        /// <summary>
        /// Appends retrieved comments to the subtree rooted of comments in this page.
        /// </summary>
        /// <param name="bundle">The bundle of comments to add.</param>
        protected void AppendComments([NotNull] CommentBundle bundle)
        {
            var topLevelComments = new List<DrawableComment>();
            var orphaned = new List<Comment>();

            foreach (var comment in bundle.Comments.Concat(bundle.IncludedComments).Concat(bundle.PinnedComments))
            {
                // Exclude possible duplicated comments.
                if (CommentDictionary.ContainsKey(comment.Id))
                    continue;

                addNewComment(comment);
            }

            // Comments whose parents were seen later than themselves can now be added.
            foreach (var o in orphaned)
                addNewComment(o);

            if (topLevelComments.Any())
            {
                LoadComponentsAsync(topLevelComments, loaded =>
                {
                    pinnedContent.AddRange(loaded.Where(d => d.Comment.Pinned));
                    content.AddRange(loaded.Where(d => !d.Comment.Pinned));

                    deletedCommentsCounter.Count.Value += topLevelComments.Select(d => d.Comment).Count(c => c.IsDeleted && c.IsTopLevel);

                    if (bundle.HasMore)
                    {
                        int loadedTopLevelComments = 0;
                        pinnedContent.Children.OfType<DrawableComment>().ForEach(p => loadedTopLevelComments++);
                        content.Children.OfType<DrawableComment>().ForEach(p => loadedTopLevelComments++);

                        moreButton.Current.Value = bundle.TopLevelCount - loadedTopLevelComments;
                        moreButton.IsLoading = false;
                    }
                    else
                    {
                        moreButton.Hide();
                    }
                }, (loadCancellation = new CancellationTokenSource()).Token);
            }

            void addNewComment(Comment comment)
            {
                var drawableComment = getDrawableComment(comment);

                if (comment.ParentId == null)
                {
                    // Comments that have no parent are added as top-level comments to the flow.
                    topLevelComments.Add(drawableComment);
                }
                else if (CommentDictionary.TryGetValue(comment.ParentId.Value, out var parentDrawable))
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

        private DrawableComment getDrawableComment(Comment comment)
        {
            if (CommentDictionary.TryGetValue(comment.Id, out var existing))
                return existing;

            return CommentDictionary[comment.Id] = new DrawableComment(comment)
            {
                ShowDeleted = { BindTarget = ShowDeleted },
                Sort = { BindTarget = Sort },
                RepliesRequested = onCommentRepliesRequested
            };
        }

        private void onCommentRepliesRequested(DrawableComment drawableComment, int page)
        {
            var req = new GetCommentsRequest(id.Value, type.Value, Sort.Value, page, drawableComment.Comment.Id);

            req.Success += response => Schedule(() => AppendComments(response));

            api.PerformAsync(req);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }

        private class NoCommentsPlaceholder : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 80;
                RelativeSizeAxes = Axes.X;
                AddRangeInternal(new Drawable[]
                {
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
