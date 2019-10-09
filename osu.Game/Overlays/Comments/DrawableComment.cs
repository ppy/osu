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
        private const float separator_height = 1.5f;
        private const int deleted_placeholder_margin = 80;

        public readonly BindableBool ShowDeleted = new BindableBool();

        private readonly BindableBool childExpanded = new BindableBool(true);

        private readonly Container childCommentsVisibilityContainer;
        private readonly Comment comment;

        public DrawableComment(Comment comment)
        {
            LinkFlowContainer username;
            FillFlowContainer childCommentsContainer;
            FillFlowContainer info;
            TextFlowContainer message;
            GridContainer content;
            VotePill votePill;

            this.comment = comment;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    content = new GridContainer
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
                                        votePill = new VotePill(comment.VotesCount)
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            AlwaysPresent = true,
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
                                    Spacing = new Vector2(0, 3),
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
                                                new ParentUsername(comment),
                                                new SpriteText
                                                {
                                                    Alpha = comment.IsDeleted? 1 : 0,
                                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                                                    Text = @"deleted",
                                                }
                                            }
                                        },
                                        message = new TextFlowContainer(s => s.Font = OsuFont.GetFont(size: 14))
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Padding = new MarginPadding { Right = message_padding }
                                        },
                                        info = new FillFlowContainer
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
                            }
                        }
                    },
                    childCommentsVisibilityContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Masking = true,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                childCommentsContainer = new FillFlowContainer
                                {
                                    Margin = new MarginPadding { Left = child_margin },
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical
                                },
                                new DeletedChildsPlaceholder(comment.GetDeletedChildsCount())
                                {
                                    Margin = new MarginPadding { Bottom = margin, Left = deleted_placeholder_margin },
                                    ShowDeleted = { BindTarget = ShowDeleted }
                                }
                            }
                        }
                    }
                }
            };

            if (comment.UserId == null)
                username.AddText(comment.LegacyName);
            else
                username.AddUserLink(comment.User);

            if (comment.EditedAt.HasValue)
            {
                info.Add(new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.GetFont(size: 12),
                    Text = $@"edited {HumanizerUtils.Humanize(comment.EditedAt.Value)} by {comment.EditedUser.Username}"
                });
            }

            if (!comment.IsDeleted)
                message.Text = comment.GetMessage();
            else
            {
                content.FadeColour(OsuColour.Gray(0.5f));
                votePill.Hide();
            }

            if (comment.IsTopLevel)
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = separator_height,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.1f)
                    }
                });

                if (comment.ChildComments.Any())
                {
                    AddInternal(new ChevronButton(comment)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding { Right = chevron_margin, Top = margin },
                        Expanded = { BindTarget = childExpanded }
                    });
                }
            }

            comment.ChildComments.ForEach(c => childCommentsContainer.Add(new DrawableComment(c)
            { ShowDeleted = { BindTarget = ShowDeleted } }));
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(onShowDeletedChanged, true);
            childExpanded.BindValueChanged(onChildExpandedChanged, true);
            base.LoadComplete();
        }

        private void onChildExpandedChanged(ValueChangedEvent<bool> expanded)
        {
            if (expanded.NewValue)
                childCommentsVisibilityContainer.AutoSizeAxes = Axes.Y;
            else
            {
                childCommentsVisibilityContainer.AutoSizeAxes = Axes.None;
                childCommentsVisibilityContainer.ResizeHeightTo(0);
            }
        }

        private void onShowDeletedChanged(ValueChangedEvent<bool> show)
        {
            if (comment.IsDeleted)
            {
                if (show.NewValue)
                    AutoSizeAxes = Axes.Y;
                else
                {
                    AutoSizeAxes = Axes.None;
                    this.ResizeHeightTo(0);
                }
            }
        }

        private class DeletedChildsPlaceholder : FillFlowContainer
        {
            public readonly BindableBool ShowDeleted = new BindableBool();

            private readonly bool canBeVisible;

            public DeletedChildsPlaceholder(int count)
            {
                canBeVisible = count != 0;

                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(3, 0);
                Alpha = 0;
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Trash,
                        Size = new Vector2(14),
                    },
                    new SpriteText
                    {
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                        Text = $@"{count} deleted comments"
                    }
                };
            }

            protected override void LoadComplete()
            {
                ShowDeleted.BindValueChanged(onShowDeletedChanged, true);
                base.LoadComplete();
            }

            private void onShowDeletedChanged(ValueChangedEvent<bool> showDeleted)
            {
                if (canBeVisible)
                    this.FadeTo(showDeleted.NewValue ? 0 : 1);
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
