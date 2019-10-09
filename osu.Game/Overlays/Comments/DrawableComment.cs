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
using osu.Framework.Graphics.Cursor;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;
        private const int child_margin = 20;
        private const int chevron_margin = 30;
        private const int message_padding = 40;
        private const int duration = 200;

        private readonly BindableBool childExpanded = new BindableBool(true);

        private readonly Container childCommentsVisibilityContainer;

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
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Horizontal = margin },
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5, 0),
                                    Children = new Drawable[]
                                    {
                                        new VotePill(comment.VotesCount)
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        },
                                        new UpdateableAvatar(comment.User)
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Size = new Vector2(avatar_size),
                                            Masking = true,
                                            CornerRadius = avatar_size / 2,
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Margin = new MarginPadding { Top = margin / 2 },
                                    Spacing = new Vector2(0, 2),
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(7, 0),
                                                    Children = new Drawable[]
                                                    {
                                                        username = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true))
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                        },
                                                        new ParentUsername(comment)
                                                    }
                                                },
                                                new ChevronButton(comment)
                                                {
                                                    Anchor = Anchor.TopRight,
                                                    Origin = Anchor.TopRight,
                                                    Margin = new MarginPadding { Right = chevron_margin },
                                                    Expanded = { BindTarget = childExpanded }
                                                }
                                            }
                                        },
                                        new TextFlowContainer(s => s.Font = OsuFont.GetFont(size: 14))
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Text = comment.GetMessage(),
                                            Padding = new MarginPadding { Right = message_padding }
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
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        new SpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.GetFont(size: 12),
                                            Text = HumanizerUtils.Humanize(comment.CreatedAt)
                                        },
                                        new RepliesButton(comment.RepliesCount)
                                        { Expanded = { BindTarget = childExpanded } },
                                    }
                                }
                            }
                        }
                    },
                    childCommentsVisibilityContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        AutoSizeDuration = duration,
                        AutoSizeEasing = Easing.OutQuint,
                        Masking = true,
                        Child = childCommentsContainer = new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = child_margin },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical
                        }
                    }
                }
            };

            if (comment.UserId == null)
            {
                username.AddText(comment.LegacyName);
            }
            else
            {
                username.AddUserLink(comment.User);
            }

            comment.ChildComments.ForEach(c =>
            {
                if (!c.IsDeleted)
                    childCommentsContainer.Add(new DrawableComment(c));
            });
        }

        protected override void LoadComplete()
        {
            childExpanded.BindValueChanged(onChildExpandedChanged, true);
            base.LoadComplete();
        }

        private void onChildExpandedChanged(ValueChangedEvent<bool> expanded)
        {
            childCommentsVisibilityContainer.ClearTransforms();

            if (expanded.NewValue)
                childCommentsVisibilityContainer.AutoSizeAxes = Axes.Y;
            else
            {
                childCommentsVisibilityContainer.AutoSizeAxes = Axes.None;
                childCommentsVisibilityContainer.ResizeHeightTo(0, duration, Easing.OutQuint);
            }
        }

        private class ChevronButton : ShowChildsButton
        {
            private readonly SpriteIcon icon;

            public ChevronButton(Comment comment)
            {
                Alpha = comment.IsTopLevel && comment.ChildComments.Any() ? 1 : 0;
                Child = icon = new SpriteIcon
                {
                    Size = new Vector2(12),
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                icon.Icon = expanded.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
            }
        }

        private class RepliesButton : ShowChildsButton
        {
            private readonly SpriteText text;
            private readonly int count;

            public RepliesButton(int count)
            {
                this.count = count;

                Alpha = count == 0 ? 0 : 1;
                Child = text = new SpriteText
                {
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                text.Text = $@"{(expanded.NewValue ? "[+]" : "[-]")} replies ({count})";
            }
        }

        private class ParentUsername : FillFlowContainer, IHasTooltip
        {
            private const int spacing = 3;

            public string TooltipText => comment.ParentComment?.GetMessage() ?? "";

            private readonly Comment comment;

            public ParentUsername(Comment comment)
            {
                this.comment = comment;

                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(spacing, 0);
                Alpha = comment.ParentId == null ? 0 : 1;
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Reply,
                        Size = new Vector2(14),
                    },
                    new SpriteText
                    {
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                        Text = comment.ParentComment?.User?.Username ?? comment.ParentComment?.LegacyName
                    }
                };
            }
        }

        private class VotePill : CircularContainer
        {
            private const int height = 20;
            private const int margin = 10;

            public VotePill(int count)
            {
                AutoSizeAxes = Axes.X;
                Height = height;
                Masking = true;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = margin },
                        Font = OsuFont.GetFont(size: 14),
                        Text = $"+{count}"
                    }
                };
            }
        }
    }
}
