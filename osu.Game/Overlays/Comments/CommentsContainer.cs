// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System.Threading;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private readonly CommentableType type;
        private readonly long id;

        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private GetCommentsRequest request;
        private CancellationTokenSource loadCancellation;
        private int currentPage;

        private readonly Box background;
        private readonly FillFlowContainer content;
        private readonly DeletedChildrenPlaceholder deletedChildrenPlaceholder;
        private readonly CommentsShowMoreButton moreButton;

        public CommentsContainer(CommentableType type, long id)
        {
            this.type = type;
            this.id = id;

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
                                                Action = getComments
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
        }

        protected override void LoadComplete()
        {
            Sort.BindValueChanged(onSortChanged, true);
            base.LoadComplete();
        }

        private void onSortChanged(ValueChangedEvent<CommentsSortCriteria> sort)
        {
            clearComments();
            getComments();
        }

        private void getComments()
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            request = new GetCommentsRequest(type, id, Sort.Value, currentPage++);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void clearComments()
        {
            currentPage = 1;
            deletedChildrenPlaceholder.DeletedCount.Value = 0;
            moreButton.IsLoading = true;
            content.Clear();
        }

        private void onSuccess(CommentBundle response)
        {
            loadCancellation = new CancellationTokenSource();

            FillFlowContainer page = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
            };

            foreach (var c in response.Comments)
            {
                if (c.IsTopLevel)
                    page.Add(new DrawableComment(c)
                    {
                        ShowDeleted = { BindTarget = ShowDeleted }
                    });
            }

            LoadComponentAsync(page, loaded =>
            {
                content.Add(loaded);

                deletedChildrenPlaceholder.DeletedCount.Value += response.Comments.Count(c => c.IsDeleted && c.IsTopLevel);

                if (response.HasMore)
                {
                    int loadedTopLevelComments = 0;
                    content.Children.OfType<FillFlowContainer>().ForEach(p => loadedTopLevelComments += p.Children.OfType<DrawableComment>().Count());

                    moreButton.Current.Value = response.TopLevelCount - loadedTopLevelComments;
                    moreButton.IsLoading = false;
                }

                moreButton.FadeTo(response.HasMore ? 1 : 0);
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
