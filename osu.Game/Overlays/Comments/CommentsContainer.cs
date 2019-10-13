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

        private readonly Box background;
        private readonly FillFlowContainer content;
        private readonly FillFlowContainer footer;
        private readonly DeletedChildsPlaceholder deletedChildsPlaceholder;

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
                                footer = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        deletedChildsPlaceholder = new DeletedChildsPlaceholder
                                        {
                                            ShowDeleted = { BindTarget = ShowDeleted }
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
            Sort.BindValueChanged(onSortChanged, true);
            base.LoadComplete();
        }

        private void onSortChanged(ValueChangedEvent<CommentsSortCriteria> sort) => getComments();

        private void getComments()
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            request = new GetCommentsRequest(type, id, Sort.Value);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(APICommentsController response)
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
                content.Clear();

                content.Add(loaded);

                int deletedComments = 0;

                response.Comments.ForEach(comment =>
                {
                    if (comment.IsDeleted && comment.IsTopLevel)
                        deletedComments++;
                });

                deletedChildsPlaceholder.DeletedCount.Value = deletedComments;
            }, loadCancellation.Token);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray2;
        }
    }
}
