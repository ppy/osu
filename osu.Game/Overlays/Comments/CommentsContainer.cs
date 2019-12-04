// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System.Threading;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Users;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private CommentBundle commentBundle;

        public CommentBundle CommentBundle
        {
            get => commentBundle;
            set
            {
                if (commentBundle == value)
                    return;

                commentBundle = value;

                onLoadStarted();
                resetComments(commentBundle);
            }
        }

        private readonly Bindable<CommentsSortCriteria> sort = new Bindable<CommentsSortCriteria>();
        private readonly Bindable<User> user = new Bindable<User>();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private GetCommentsRequest request;
        private int currentPage;
        private CommentBundleParameters parameters;
        private CancellationTokenSource loadCancellation;

        private readonly CommentsHeader commentsHeader;
        private readonly Box background;
        private readonly Container noCommentsPlaceholder;
        private readonly Box placeholderBackground;
        private readonly FillFlowContainer content;
        private readonly DeletedChildrenPlaceholder deletedChildrenPlaceholder;
        private readonly CommentsShowMoreButton moreButton;
        private readonly DimmedLoadingLayer loadingLayer;
        private readonly TotalCommentsCounter totalCommentsCounter;

        public CommentsContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        totalCommentsCounter = new TotalCommentsCounter(),
                        commentsHeader = new CommentsHeader
                        {
                            Sort = { BindTarget = sort }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                noCommentsPlaceholder = new Container
                                {
                                    Height = 80,
                                    RelativeSizeAxes = Axes.X,
                                    Alpha = 0,
                                    Children = new Drawable[]
                                    {
                                        placeholderBackground = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Margin = new MarginPadding { Left = 50 },
                                            Text = @"No comments yet."
                                        }
                                    }
                                },
                                content = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                },
                                loadingLayer = new DimmedLoadingLayer
                                {
                                    Alpha = 0,
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(0.2f)
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        deletedChildrenPlaceholder = new DeletedChildrenPlaceholder
                                        {
                                            ShowDeleted = { BindTarget = commentsHeader.ShowDeleted }
                                        },
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Child = moreButton = new CommentsShowMoreButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Margin = new MarginPadding(5),
                                                Action = fetchComments,
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
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray2;
            placeholderBackground.Colour = colours.Gray3;

            user.BindTo(api.LocalUser);
        }

        protected override void LoadComplete()
        {
            sort.BindValueChanged(onSortChanged);
            user.BindValueChanged(_ => sort.TriggerChange());
            base.LoadComplete();
        }

        public void ShowComments(CommentableType type, long id)
        {
            parameters = new CommentBundleParameters(type, id);
            sort.TriggerChange();
        }

        private void onSortChanged(ValueChangedEvent<CommentsSortCriteria> sort)
        {
            if (parameters == null)
                return;

            onLoadStarted();
            fetchComments();
        }

        private void fetchComments()
        {
            request = new GetCommentsRequest(parameters, sort.Value, ++currentPage);
            request.Success += response =>
            {
                if (currentPage == 1)
                    resetComments(response);
                else
                    AddComments(response, false);
            };
            api.PerformAsync(request);
        }

        private void onLoadStarted()
        {
            request?.Cancel();
            currentPage = 0;
            loadCancellation?.Cancel();
            moreButton.IsLoading = true;

            if (content.Children.Any() || noCommentsPlaceholder.IsPresent)
                loadingLayer.Show();
        }

        private void resetComments(CommentBundle comments)
        {
            if (comments == null)
            {
                content.Clear();
                deletedChildrenPlaceholder.DeletedCount.Value = 0;
                totalCommentsCounter.Current.Value = 0;
                noCommentsPlaceholder.Hide();
                loadingLayer.Hide();
                moreButton.IsLoading = true;
                moreButton.Show();
                return;
            }

            if (!comments.Comments.Any())
            {
                content.Clear();
                deletedChildrenPlaceholder.DeletedCount.Value = 0;
                noCommentsPlaceholder.Show();
                onLoadFinished(comments);
                return;
            }

            AddComments(comments, true);
        }

        protected void AddComments(CommentBundle comments, bool reset)
        {
            loadCancellation = new CancellationTokenSource();

            var page = createCommentsPage(comments);

            LoadComponentAsync(page, loaded =>
            {
                if (reset)
                {
                    content.Clear();
                    deletedChildrenPlaceholder.DeletedCount.Value = getDeletedComments(comments);
                }
                else
                {
                    deletedChildrenPlaceholder.DeletedCount.Value += getDeletedComments(comments);
                }

                noCommentsPlaceholder.Hide();

                content.Add(loaded);

                onLoadFinished(comments);
            }, loadCancellation.Token);
        }

        private int getDeletedComments(CommentBundle comments) => comments.Comments.Count(c => c.IsDeleted && c.IsTopLevel);

        private void onLoadFinished(CommentBundle comments)
        {
            totalCommentsCounter.Current.Value = comments.Total;
            loadingLayer.Hide();
            updateMoreButtonState(comments);
        }

        private FillFlowContainer createCommentsPage(CommentBundle response)
        {
            var page = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
            };

            foreach (var c in response.Comments)
            {
                if (c.IsTopLevel)
                {
                    page.Add(new DrawableComment(c)
                    {
                        ShowDeleted = { BindTarget = commentsHeader.ShowDeleted }
                    });
                }
            }

            return page;
        }

        private void updateMoreButtonState(CommentBundle comments)
        {
            moreButton.IsLoading = false;

            if (comments.HasMore)
            {
                int loadedTopLevelComments = 0;
                content.Children.OfType<FillFlowContainer>().ForEach(p => loadedTopLevelComments += p.Children.OfType<DrawableComment>().Count());

                moreButton.Current.Value = comments.TopLevelCount - loadedTopLevelComments;
            }

            moreButton.FadeTo(comments.HasMore ? 1 : 0);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
