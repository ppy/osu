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
using osu.Game.Users;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private readonly Bindable<CommentableType> type = new Bindable<CommentableType>();
        private readonly BindableLong id = new BindableLong();

        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        protected readonly IBindable<User> User = new Bindable<User>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetCommentsRequest request;
        private ScheduledDelegate scheduledCommentsLoad;
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
            clearComments();
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
            request.Success += res => scheduledCommentsLoad = Schedule(() => onSuccess(res));
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
                ShowDeleted = { BindTarget = ShowDeleted },
                Sort = { BindTarget = Sort },
                Type = { BindTarget = type },
                CommentableId = { BindTarget = id }
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
