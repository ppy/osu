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

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private readonly CommentableType type;
        private readonly long id;

        public readonly Bindable<SortCommentsBy> Sort = new Bindable<SortCommentsBy>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private GetCommentsRequest request;

        private readonly Box background;
        private readonly FillFlowContainer content;

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

        private void onSortChanged(ValueChangedEvent<SortCommentsBy> sort) => getComments();

        private void getComments()
        {
            request?.Cancel();
            content.Clear();
            request = new GetCommentsRequest(type, id, Sort.Value);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(APICommentsController response)
        {
            foreach (var c in response.Comments)
            {
                if (c.IsTopLevel)
                    content.Add(new DrawableComment(c)
                    {
                        ShowDeleted = { BindTarget = ShowDeleted }
                    });
            }

            int deletedComments = 0;

            response.Comments.ForEach(comment =>
            {
                if (comment.IsDeleted && comment.IsTopLevel)
                    deletedComments++;
            });

            content.Add(new DeletedChildsPlaceholder(deletedComments)
            {
                ShowDeleted = { BindTarget = ShowDeleted }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray3;
        }
    }
}
