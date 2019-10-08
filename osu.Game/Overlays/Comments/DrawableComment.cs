// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Utils;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;
        private const int child_margin = 20;

        public DrawableComment(Comment comment)
        {
            LinkFlowContainer username;
            FillFlowContainer childCommentsContainer;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding(margin),
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new UpdateableAvatar(comment.User)
                                {
                                    Size = new Vector2(avatar_size),
                                    Margin = new MarginPadding { Horizontal = margin },
                                    Masking = true,
                                    CornerRadius = avatar_size / 2,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Margin = new MarginPadding { Top = margin / 2 },
                                    Spacing = new Vector2(0, 2),
                                    Children = new Drawable[]
                                    {
                                        username = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true))
                                        {
                                            AutoSizeAxes = Axes.Both,
                                        },
                                        new TextFlowContainer(s => s.Font = OsuFont.GetFont(size: 14))
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Text = comment.GetMessage()
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: 12),
                                    Text = HumanizerUtils.Humanize(comment.CreatedAt)
                                }
                            }
                        }
                    },
                    childCommentsContainer = new FillFlowContainer
                    {
                        Margin = new MarginPadding { Left = child_margin },
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical
                    }
                }
            };

            username.AddUserLink(comment.User);

            comment.ChildComments.ForEach(c =>
            {
                if (!c.IsDeleted)
                    childCommentsContainer.Add(new DrawableComment(c));
            });
        }
    }
}
