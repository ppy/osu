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
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;

        public readonly BindableBool ShowDeleted = new BindableBool();

        private readonly BindableBool childrenExpanded = new BindableBool(true);

        private FillFlowContainer childCommentsVisibilityContainer;
        private readonly Comment comment;

        public DrawableComment(Comment comment)
        {
            this.comment = comment;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LinkFlowContainer username;
            FillFlowContainer childCommentsContainer;
            DeletedCommentsCounter deletedCommentsCounter;
            FillFlowContainer info;
            LinkFlowContainer message;
            GridContainer content;
            VotePill votePill;

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
                                                CornerExponent = 2,
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
                                                    new OsuSpriteText
                                                    {
                                                        Alpha = comment.IsDeleted ? 1 : 0,
                                                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                                                        Text = @"deleted",
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
                                                Colour = OsuColour.Gray(0.7f),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Text = HumanizerUtils.Humanize(comment.CreatedAt)
                                                    },
                                                    new RepliesButton(comment.RepliesCount)
                                                    {
                                                        Expanded = { BindTarget = childrenExpanded }
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
                            deletedCommentsCounter = new DeletedCommentsCounter
                            {
                                ShowDeleted = { BindTarget = ShowDeleted }
                            }
                        }
                    }
                }
            };

            deletedCommentsCounter.Count.Value = comment.DeletedChildrenCount;

            if (comment.UserId.HasValue)
                username.AddUserLink(comment.User);
            else
                username.AddText(comment.LegacyName);

            if (comment.EditedAt.HasValue)
            {
                info.Add(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
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

                if (comment.ChildComments.Any())
                {
                    AddInternal(new ChevronButton(comment)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding { Right = 30, Top = margin },
                        Expanded = { BindTarget = childrenExpanded }
                    });
                }
            }

            comment.ChildComments.ForEach(c => childCommentsContainer.Add(new DrawableComment(c)
            {
                ShowDeleted = { BindTarget = ShowDeleted }
            }));
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(show =>
            {
                if (comment.IsDeleted)
                    this.FadeTo(show.NewValue ? 1 : 0);
            }, true);
            childrenExpanded.BindValueChanged(expanded => childCommentsVisibilityContainer.FadeTo(expanded.NewValue ? 1 : 0), true);
            base.LoadComplete();
        }

        private class ChevronButton : ShowChildrenButton
        {
            private readonly SpriteIcon icon;

            public ChevronButton(Comment comment)
            {
                Alpha = comment.IsTopLevel && comment.ChildComments.Any() ? 1 : 0;
                Child = icon = new SpriteIcon
                {
                    Size = new Vector2(12),
                    Colour = OsuColour.Gray(0.7f)
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                icon.Icon = expanded.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
            }
        }

        private class RepliesButton : ShowChildrenButton
        {
            private readonly SpriteText text;
            private readonly int count;

            public RepliesButton(int count)
            {
                this.count = count;

                Alpha = count == 0 ? 0 : 1;
                Child = text = new OsuSpriteText
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
                    new OsuSpriteText
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
