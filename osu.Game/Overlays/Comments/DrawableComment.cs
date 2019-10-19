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
using System.Linq;
using osu.Game.Online.Chat;
using System.Collections.Generic;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;

        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();

        private readonly BindableBool childrenExpanded = new BindableBool(true);
        private readonly Bindable<List<Comment>> childComments = new Bindable<List<Comment>>();
        private readonly List<Comment> loadedChildren = new List<Comment>();

        private readonly FillFlowContainer childCommentsVisibilityContainer;
        private readonly FillFlowContainer childCommentsContainer;
        private readonly DeletedChildrenPlaceholder deletedChildrenPlaceholder;
        private readonly Comment comment;

        public DrawableComment(Comment comment)
        {
            LinkFlowContainer username;
            FillFlowContainer info;
            LinkFlowContainer message;
            GridContainer content;
            VotePill votePill;

            this.comment = comment;
            childComments.Value = comment.ChildComments;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(margin) { Left = margin + 5 },
                        Child = content = new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
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
                                            new Container
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 40,
                                                AutoSizeAxes = Axes.Y,
                                                Child = votePill = new VotePill(comment)
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                }
                                            },
                                            new UpdateableAvatar(comment.User)
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(avatar_size),
                                                Masking = true,
                                                CornerRadius = avatar_size / 2f,
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
                                            new Container
                                            {
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
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
                                                                Alpha = comment.IsDeleted ? 1 : 0,
                                                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                                                                Text = @"deleted",
                                                            }
                                                        }
                                                    },
                                                    new ChevronButton(comment)
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                        Expanded = { BindTarget = childrenExpanded },
                                                        ChildComments = { BindTarget = childComments }
                                                    }
                                                }
                                            },
                                            message = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14))
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Right = 40 }
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
                                                        Text = HumanizerUtils.Humanize(comment.CreatedAt),
                                                        Colour = OsuColour.Gray(0.7f),
                                                    },
                                                    new RepliesButton(comment)
                                                    {
                                                        Expanded = { BindTarget = childrenExpanded },
                                                        ChildComments = { BindTarget = childComments },
                                                    },
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    childCommentsVisibilityContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            childCommentsContainer = new FillFlowContainer
                            {
                                Padding = new MarginPadding { Left = 20 },
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical
                            },
                            deletedChildrenPlaceholder = new DeletedChildrenPlaceholder
                            {
                                ShowDeleted = { BindTarget = ShowDeleted }
                            },
                            new ShowMoreRepliesButton(comment)
                            {
                                ChildComments = { BindTarget = childComments },
                                Sort = { BindTarget = Sort }
                            }
                        }
                    }
                }
            };

            if (comment.UserId.HasValue)
                username.AddUserLink(comment.User);
            else
                username.AddText(comment.LegacyName);

            if (!childComments.Value.Any() && comment.RepliesCount > 0)
                info.Add(new LoadRepliesButton(comment)
                {
                    ChildComments = { BindTarget = childComments },
                    Sort = { BindTarget = Sort }
                });

            if (comment.EditedAt.HasValue)
            {
                info.Add(new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Colour = OsuColour.Gray(0.7f),
                    Font = OsuFont.GetFont(size: 12),
                    Text = $@"edited {HumanizerUtils.Humanize(comment.EditedAt.Value)} by {comment.EditedUser.Username}"
                });
            }

            if (comment.HasMessage)
            {
                var formattedSource = MessageFormatter.FormatText(comment.GetMessage);
                message.AddLinks(formattedSource.Text, formattedSource.Links);
            }

            if (comment.IsDeleted)
            {
                content.FadeColour(OsuColour.Gray(0.5f));
                votePill.Hide();
            }

            if (comment.IsTopLevel)
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.1f)
                    }
                });
            }
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(show =>
            {
                if (comment.IsDeleted)
                    this.FadeTo(show.NewValue ? 1 : 0);
            }, true);
            childrenExpanded.BindValueChanged(expanded => childCommentsVisibilityContainer.FadeTo(expanded.NewValue ? 1 : 0), true);
            childComments.BindValueChanged(children =>
            {
                children.NewValue.ForEach(c =>
                {
                    if (!loadedChildren.Contains(c))
                    {
                        childCommentsContainer.Add(new DrawableComment(c)
                        {
                            ShowDeleted = { BindTarget = ShowDeleted }
                        });
                        loadedChildren.Add(c);
                    }
                });

                deletedChildrenPlaceholder.DeletedCount.Value = loadedChildren.Count(c => c.IsDeleted);
            }, true);
            base.LoadComplete();
        }

        private class ChevronButton : ShowChildrenButton
        {
            private readonly SpriteIcon icon;
            private readonly Comment comment;

            public ChevronButton(Comment comment)
            {
                this.comment = comment;

                Child = icon = new SpriteIcon
                {
                    Size = new Vector2(12),
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                icon.Icon = expanded.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
            }

            protected override void OnChildrenChanged(ValueChangedEvent<List<Comment>> children)
            {
                Alpha = comment.IsTopLevel && children.NewValue.Any() ? 1 : 0;
            }
        }

        private class RepliesButton : ShowChildrenButton
        {
            private readonly SpriteText text;
            private readonly int repliesCount;

            public RepliesButton(Comment comment)
            {
                repliesCount = comment.RepliesCount;

                Child = text = new SpriteText
                {
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                text.Text = $@"{(expanded.NewValue ? "[+]" : "[-]")} replies ({repliesCount})";
            }

            protected override void OnChildrenChanged(ValueChangedEvent<List<Comment>> children)
            {
                Alpha = children.NewValue.Count == 0 || repliesCount == 0 ? 0 : 1;
            }
        }

        private class ParentUsername : FillFlowContainer, IHasTooltip
        {
            public string TooltipText => getParentMessage();

            private readonly Comment parentComment;

            public ParentUsername(Comment comment)
            {
                parentComment = comment.ParentComment;

                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(3, 0);
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
                        Text = parentComment?.User?.Username ?? parentComment?.LegacyName
                    }
                };
            }

            private string getParentMessage()
            {
                if (parentComment == null)
                    return string.Empty;

                return parentComment.HasMessage ? parentComment.GetMessage : parentComment.IsDeleted ? @"deleted" : string.Empty;
            }
        }
    }
}
