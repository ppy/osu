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
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays
{
    public class CommentsContainer : CompositeDrawable
    {
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

        private void onSuccess(APIComments response)
        {
            content.Clear();

            foreach (var c in response.Comments)
            {
                if (!c.IsDeleted)
                    createDrawableComment(c);
            }
        }

        private void createDrawableComment(Comment comment)
        {
            content.Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 70,
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = comment.GetMessage(),
                    },
                    new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray1,
                        }
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray3;
        }

        private class CommentsHeader : CompositeDrawable
        {
            private const int height = 40;
            private const int spacing = 10;
            private const int padding = 50;

            public readonly Bindable<SortCommentsBy> Sort = new Bindable<SortCommentsBy>();

            private readonly Box background;

            public CommentsHeader()
            {
                RelativeSizeAxes = Axes.X;
                Height = height;
                AddRangeInternal(new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = padding },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(spacing, 0),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 14),
                                        Text = @"Sort by"
                                    }
                                }
                            }
                        }
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Gray4;
            }
        }
    }
}
