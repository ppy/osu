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
using osu.Game.Users;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private CommentableType type;
        private long? id;

        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        protected readonly Bindable<User> User = new Bindable<User>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetCommentsRequest request;
        private CancellationTokenSource loadCancellation;
        private int currentPage;

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
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background4
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        deletedCommentsCounter = new DeletedCommentsCounter
                                        {
                                            ShowDeleted = { BindTarget = ShowDeleted }
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
            this.type = type;
            this.id = id;

            if (!IsLoaded)
                return;

            // only reset when changing ID/type. other refetch ops are generally just changing sort order.
            commentCounter.Current.Value = 0;

            refetchComments();
        }

        private void refetchComments()
        {
            clearComments();
            getComments();
        }

        private void getComments()
        {
            if (!id.HasValue)
                return;

            request?.Cancel();
            loadCancellation?.Cancel();
            request = new GetCommentsRequest(type, id.Value, Sort.Value, currentPage++);
            request.Success += onSuccess;
            api.PerformAsync(request);
        }

        private void clearComments()
        {
            currentPage = 1;
            deletedCommentsCounter.Count.Value = 0;
            moreButton.Show();
            moreButton.IsLoading = true;
            content.Clear();
        }

        private void onSuccess(CommentBundle response)
        {
            loadCancellation = new CancellationTokenSource();

            LoadComponentAsync(new CommentsPage(response)
            {
                ShowDeleted = { BindTarget = ShowDeleted }
            }, loaded =>
            {
                content.Add(loaded);

                deletedCommentsCounter.Count.Value += response.Comments.Count(c => c.IsDeleted && c.IsTopLevel);

                if (response.HasMore)
                {
                    int loadedTopLevelComments = 0;
                    content.Children.OfType<FillFlowContainer>().ForEach(p => loadedTopLevelComments += p.Children.OfType<DrawableComment>().Count());

                    moreButton.Current.Value = response.TopLevelCount - loadedTopLevelComments;
                    moreButton.IsLoading = false;
                }
                else
                {
                    moreButton.Hide();
                }

                commentCounter.Current.Value = response.Total;
            }, loadCancellation.Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
