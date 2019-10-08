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
        private const float separator_height = 1.5f;

        private readonly CommentableType type;
        private readonly long id;

        public readonly Bindable<SortCommentsBy> Sort = new Bindable<SortCommentsBy>();

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
                            Sort = { BindTarget = Sort }
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
            request = new GetCommentsRequest(type, id, Sort.Value);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(APICommentsController response)
        {
            content.Clear();

            foreach (var c in response.Comments)
            {
                if (!c.IsDeleted && c.IsTopLevel)
                    content.AddRange(new Drawable[]
                    {
                        new DrawableComment(c),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = separator_height,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colours.Gray1,
                            }
                        }
                    });
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray3;
        }
    }
}
