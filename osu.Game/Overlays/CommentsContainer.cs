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

namespace osu.Game.Overlays
{
    public class CommentsContainer : CompositeDrawable
    {
        private readonly CommentableType type;
        private readonly long id;

        public readonly Bindable<SortCommentsBy> Sort = new Bindable<SortCommentsBy>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly CommentsHeader header;
        private readonly Box background;

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
                header = new CommentsHeader
                {
                    Sort = { BindTarget = Sort }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
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
