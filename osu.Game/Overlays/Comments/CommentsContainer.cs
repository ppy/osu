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
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        public readonly BindableBool ShowDeleted = new BindableBool();

        private CommentBundle commentBundle;

        public CommentBundle CommentBundle
        {
            get => commentBundle;
            set
            {
                if (commentBundle == value)
                    return;

                commentBundle = value;

                OnLoadingStarted();
                AddComments(commentBundle);
            }
        }

        protected readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        private readonly Bindable<User> user = new Bindable<User>();

        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private CancellationTokenSource loadCancellation;

        private readonly Box background;
        private readonly Container noCommentsPlaceholder;
        private readonly Box placeholderBackground;
        private readonly FillFlowContainer content;
        private readonly DeletedChildrenPlaceholder deletedChildrenPlaceholder;
        private readonly CommentsShowMoreButton moreButton;
        private readonly DimmedLoadingLayer loadingLayer;

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
                        new CommentsHeader
                        {
                            Sort = { BindTarget = Sort },
                            ShowDeleted = { BindTarget = ShowDeleted }
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
                                                Action = OnShowMoreAction,
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

            user.BindTo(API.LocalUser);
        }

        protected override void LoadComplete()
        {
            Sort.BindValueChanged(OnSortChanged);
            user.BindValueChanged(OnUserChanged);
            base.LoadComplete();
        }

        protected virtual void OnSortChanged(ValueChangedEvent<CommentsSortCriteria> sort)
        {
        }

        protected virtual void OnUserChanged(ValueChangedEvent<User> user)
        {
            OnLoadingStarted();
            AddComments(commentBundle);
        }

        protected virtual void OnShowMoreAction()
        {
        }

        protected virtual void OnLoadingStarted()
        {
            loadCancellation?.Cancel();
            moreButton.IsLoading = true;

            if (content.Children.Any() || noCommentsPlaceholder.IsPresent)
                loadingLayer.Show();
        }

        protected void AddComments(CommentBundle comments, bool clearContent = true)
        {
            if (comments == null)
            {
                content.Clear();
                deletedChildrenPlaceholder.DeletedCount.Value = 0;
                noCommentsPlaceholder.Hide();
                onLoadingFinished(comments);
                return;
            }

            if (!comments.Comments.Any())
            {
                noCommentsPlaceholder.Show();
                onLoadingFinished(comments);
                return;
            }

            loadCancellation = new CancellationTokenSource();

            var page = createCommentsPage(comments);

            LoadComponentAsync(page, loaded =>
            {
                if (clearContent)
                    content.Clear();

                noCommentsPlaceholder.Hide();

                content.Add(loaded);

                deletedChildrenPlaceholder.DeletedCount.Value += comments.Comments.Count(c => c.IsDeleted && c.IsTopLevel);

                onLoadingFinished(comments);
            }, loadCancellation.Token);
        }

        private void onLoadingFinished(CommentBundle comments)
        {
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
                        ShowDeleted = { BindTarget = ShowDeleted }
                    });
                }
            }

            return page;
        }

        private void updateMoreButtonState(CommentBundle comments)
        {
            if (comments == null)
            {
                moreButton.IsLoading = true;
                moreButton.Show();
                return;
            }

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
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
