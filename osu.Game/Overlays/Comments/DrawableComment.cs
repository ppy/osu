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
using osu.Framework.Extensions.IEnumerableExtensions;
using System.Collections.Generic;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;

        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();

        private readonly BindableBool showReplies = new BindableBool(true);
        private readonly BindableList<Comment> replies = new BindableList<Comment>();
        private readonly BindableInt currentPage = new BindableInt();

        private readonly FillFlowContainer repliesVisibilityContainer;
        private readonly FillFlowContainer repliesContainer;
        private readonly DeletedCommentsPlaceholder deletedCommentsPlaceholder;
        private readonly ChevronButton chevronButton;
        private readonly RepliesButton repliesButton;
        private readonly LoadRepliesButton loadRepliesButton;
        private readonly ShowMoreRepliesButton showMoreRepliesButton;
        private readonly Comment comment;

        public DrawableComment(Comment comment)
        {
            LinkFlowContainer username;
            FillFlowContainer info;
            LinkFlowContainer message;
            GridContainer content;
            VotePill votePill;

            this.comment = comment;

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
                                            new Container
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(avatar_size),
                                                Masking = true,
                                                CornerRadius = avatar_size / 2f,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = OsuColour.Gray(0.2f)
                                                    },
                                                    new UpdateableAvatar(comment.User)
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                }
                                            }
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
                                                    chevronButton = new ChevronButton
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                        Expanded = { BindTarget = showReplies },
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
                                                    repliesButton = new RepliesButton(comment)
                                                    {
                                                        Expanded = { BindTarget = showReplies },
                                                    },
                                                    loadRepliesButton = new LoadRepliesButton(comment)
                                                    {
                                                        Sort = { BindTarget = Sort },
                                                        CurrentPage = { BindTarget = currentPage }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    repliesVisibilityContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            repliesContainer = new FillFlowContainer
                            {
                                Padding = new MarginPadding { Left = 20 },
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical
                            },
                            deletedCommentsPlaceholder = new DeletedCommentsPlaceholder
                            {
                                ShowDeleted = { BindTarget = ShowDeleted }
                            },
                            showMoreRepliesButton = new ShowMoreRepliesButton(comment)
                            {
                                Sort = { BindTarget = Sort },
                                CurrentPage = { BindTarget = currentPage }
                            }
                        }
                    }
                }
            };

            if (comment.UserId.HasValue)
                username.AddUserLink(comment.User);
            else
                username.AddText(comment.LegacyName);

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
                var formattedSource = MessageFormatter.FormatText(comment.Message);
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

            replies.AddRange(comment.Replies);
            repliesContainer.Add(createRepliesPage(replies));
            updateButtonsState();

            loadRepliesButton.Replies.BindTo(replies);
            showMoreRepliesButton.Replies.BindTo(replies);
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(show =>
            {
                if (comment.IsDeleted)
                    this.FadeTo(show.NewValue ? 1 : 0);
            }, true);

            showReplies.BindValueChanged(show => repliesVisibilityContainer.FadeTo(show.NewValue ? 1 : 0), true);

            replies.ItemsAdded += onChildrenAdded;
            loadRepliesButton.OnCommentsReceived += replies.AddRange;
            showMoreRepliesButton.OnCommentsReceived += replies.AddRange;

            base.LoadComplete();
        }

        private void onChildrenAdded(IEnumerable<Comment> children)
        {
            LoadComponentAsync(createRepliesPage(children), loaded =>
            {
                repliesContainer.Add(loaded);
                updateButtonsState();
            });
        }

        private FillFlowContainer<DrawableComment> createRepliesPage(IEnumerable<Comment> replies)
        {
            var page = new FillFlowContainer<DrawableComment>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
            };

            replies.ForEach(c =>
            {
                c.ParentComment = comment;
                page.Add(new DrawableComment(c)
                {
                    ShowDeleted = { BindTarget = ShowDeleted },
                    Sort = { BindTarget = Sort }
                });
            });

            return page;
        }

        private void updateButtonsState()
        {
            deletedCommentsPlaceholder.DeletedCount.Value = replies.Count(c => c.IsDeleted);

            chevronButton.FadeTo(comment.IsTopLevel && replies.Any() ? 1 : 0);
            repliesButton.FadeTo(replies.Any() ? 1 : 0);
            loadRepliesButton.FadeTo(replies.Any() || comment.RepliesCount == 0 ? 0 : 1);
            showMoreRepliesButton.FadeTo((!replies.Any() && comment.RepliesCount > 0) || replies.Count == comment.RepliesCount ? 0 : 1);

            loadRepliesButton.IsLoading = showMoreRepliesButton.IsLoading = false;
        }

        private class ChevronButton : ShowRepliesButton
        {
            private readonly SpriteIcon icon;

            public ChevronButton()
            {
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

        private class RepliesButton : ShowRepliesButton
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
                text.Text = $@"{(expanded.NewValue ? "[-]" : "[+]")} replies ({repliesCount})";
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

                return parentComment.HasMessage ? parentComment.Message : parentComment.IsDeleted ? @"deleted" : string.Empty;
            }
        }
    }
}
