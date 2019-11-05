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
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private GetCommentsRequest request;
        private CancellationTokenSource loadCancellation;
        private int currentPage;
        private CommentBundleParameters parameters;

        private readonly Box background;
        private readonly Container noCommentsPlaceholder;
        private readonly Box placeholderBackground;
        private readonly FillFlowContainer content;
        private readonly DeletedChildrenPlaceholder deletedChildrenPlaceholder;
        private readonly CommentsShowMoreButton moreButton;
        private readonly SpriteText totalCounter;
        private readonly Container totalCounterContainer;

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
                        totalCounterContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 50,
                            Alpha = 0,
                            Child = new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Margin = new MarginPadding { Left = 50 },
                                Spacing = new Vector2(5, 0),
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.GetFont(size: 20, italics: true),
                                        Text = @"Comments"
                                    },
                                    new CircularContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = OsuColour.Gray(0.05f)
                                            },
                                            totalCounter = new SpriteText
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold)
                                            }
                                        },
                                    }
                                }
                            },
                        },
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
                                placeholderBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
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

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray2;
            placeholderBackground.Colour = colours.Gray3;
            totalCounter.Colour = colours.BlueLighter;
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

        private void updateComments(bool hideTotalCounter = true)
        {
            if (parameters == null)
                return;

            clearComments(hideTotalCounter);
            getComments();
        }

        private void clearComments(bool hideTotalCounter)
        {
            if (hideTotalCounter)
                totalCounterContainer.Hide();

            request?.Cancel();
            loadCancellation?.Cancel();
            currentPage = 1;
            deletedChildrenPlaceholder.DeletedCount.Value = 0;
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
            setTotalCounterAmount(response.Total);

            if (!response.Comments.Any())
            {
                noCommentsPlaceholder.Show();
                moreButton.IsLoading = false;
                moreButton.Hide();
                return;
            }

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
                else
                {
                    moreButton.Hide();
                }
            }, loadCancellation.Token);
        }

        private void setTotalCounterAmount(int amount)
        {
            totalCounter.Text = amount.ToString("N0");
            totalCounterContainer.Show();
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
