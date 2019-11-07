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
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetCommentsRequest request;
        private CancellationTokenSource loadCancellation;
        private int currentPage;
        private CommentBundleParameters parameters;
        private TotalCommentsCounter totalCommentsCounter;
        private Container noCommentsPlaceholder;
        private FillFlowContainer content;
        private DeletedCommentsPlaceholder deletedCommentsPlaceholder;
        private CommentsShowMoreButton moreButton;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.CommentsGrayDark,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        totalCommentsCounter = new TotalCommentsCounter(),
                        new CommentsHeader
                        {
                            Sort = { BindTarget = Sort },
                            ShowDeleted = { BindTarget = ShowDeleted }
                        },
                        noCommentsPlaceholder = new Container
                        {
                            Height = 80,
                            RelativeSizeAxes = Axes.X,
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colours.CommentsGray
                                },
                                new SpriteText
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
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colours.CommentsGray
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        deletedCommentsPlaceholder = new DeletedCommentsPlaceholder
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
        }

        protected override void LoadComplete()
        {
            Sort.BindValueChanged(_ => updateComments(false));
            base.LoadComplete();
        }

        public void ShowComments(CommentableType type, long id)
        {
            parameters = new CommentBundleParameters(type, id);
            updateComments();
        }

        public void ShowComments(CommentBundle commentBundle)
        {
            parameters = null;
            clearComments(true);
            onSuccess(commentBundle);
        }

        private void updateComments(bool resetCommentsCounter = true)
        {
            if (parameters == null)
                return;

            clearComments(resetCommentsCounter);
            getComments();
        }

        private void clearComments(bool resetCommentsCounter)
        {
            if (resetCommentsCounter)
                totalCommentsCounter.SetValue(0);

            request?.Cancel();
            loadCancellation?.Cancel();
            currentPage = 1;
            deletedCommentsPlaceholder.DeletedCount.Value = 0;
            noCommentsPlaceholder.Hide();
            content.Clear();
            moreButton.IsLoading = true;
            moreButton.Show();
        }

        private void getComments()
        {
            if (parameters == null)
                return;

            request = new GetCommentsRequest(parameters, Sort.Value, currentPage++);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(CommentBundle response)
        {
            if (!response.Comments.Any())
            {
                noCommentsPlaceholder.Show();
                moreButton.IsLoading = false;
                moreButton.Hide();
                return;
            }

            loadCancellation = new CancellationTokenSource();

            var page = createCommentsPage(response);

            LoadComponentAsync(page, loaded =>
            {
                content.Add(loaded);

                deletedCommentsPlaceholder.DeletedCount.Value += response.Comments.Count(c => c.IsDeleted && c.IsTopLevel);

                updateMoreButtonState(response);

                totalCommentsCounter.SetValue(response.Total);
            }, loadCancellation.Token);
        }

        private FillFlowContainer createCommentsPage(CommentBundle response)
        {
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
                        ShowDeleted = { BindTarget = ShowDeleted },
                        Sort = { BindTarget = Sort }
                    });
            }

            return page;
        }

        private void updateMoreButtonState(CommentBundle response)
        {
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
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
