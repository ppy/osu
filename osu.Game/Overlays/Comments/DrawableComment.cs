﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using System.Linq;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Framework.Allocation;
using osuTK.Graphics;
using System.Collections.Generic;
using System;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.IEnumerableExtensions;
using System.Collections.Specialized;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;

        public Action<DrawableComment, int> RepliesRequested;
        public Action<DrawableComment, CommentBundle> NewReplyReceived;

        public readonly Comment Comment;

        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        private readonly Dictionary<long, Comment> loadedReplies = new Dictionary<long, Comment>();

        public readonly BindableList<DrawableComment> Replies = new BindableList<DrawableComment>();

        private readonly BindableBool childrenExpanded = new BindableBool(true);
        private readonly BindableBool replyEditorVisible = new BindableBool();

        private int currentPage;

        private FillFlowContainer childCommentsVisibilityContainer;
        private FillFlowContainer childCommentsContainer;
        private LoadMoreCommentsButton loadMoreCommentsButton;
        private ShowMoreButton showMoreButton;
        private RepliesButton repliesButton;
        private ChevronButton chevronButton;
        private DeletedCommentsCounter deletedCommentsCounter;

        public DrawableComment(Comment comment)
        {
            Comment = comment;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OverlayColourProvider colourProvider)
        {
            LinkFlowContainer username;
            FillFlowContainer info;
            LinkFlowContainer message;
            GridContainer content;
            VotePill votePill;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
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
                                                    Child = votePill = new VotePill(Comment)
                                                    {
                                                        Anchor = Anchor.CentreRight,
                                                        Origin = Anchor.CentreRight,
                                                    }
                                                },
                                                new UpdateableAvatar(Comment.User)
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
                                                        new ParentUsername(Comment),
                                                        new OsuSpriteText
                                                        {
                                                            Alpha = Comment.IsDeleted ? 1 : 0,
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
                                                    Children = new Drawable[]
                                                    {
                                                        new OsuSpriteText
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Font = OsuFont.GetFont(size: 12),
                                                            Colour = colourProvider.Foreground1,
                                                            Text = HumanizerUtils.Humanize(Comment.CreatedAt)
                                                        },
                                                        new ReplyButton
                                                        {
                                                            Action = replyEditorVisible.Toggle,
                                                            Alpha = api.IsLoggedIn && !Comment.IsDeleted ? 1 : 0
                                                        },
                                                        repliesButton = new RepliesButton(Comment.RepliesCount)
                                                        {
                                                            Expanded = { BindTarget = childrenExpanded }
                                                        },
                                                        loadMoreCommentsButton = new LoadMoreCommentsButton
                                                        {
                                                            Action = () => RepliesRequested(this, ++currentPage)
                                                        }
                                                    }
                                                },
                                                new ReplyEditor(Comment)
                                                {
                                                    IsVisible = { BindTarget = replyEditorVisible },
                                                    ReplyReceived = reply => NewReplyReceived?.Invoke(this, reply)
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
                                },
                                showMoreButton = new ShowMoreButton
                                {
                                    Action = () => RepliesRequested(this, ++currentPage)
                                }
                            }
                        },
                    }
                },
                chevronButton = new ChevronButton
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 30, Top = margin },
                    Expanded = { BindTarget = childrenExpanded },
                    Alpha = 0
                }
            };

            if (Comment.UserId.HasValue)
                username.AddUserLink(Comment.User);
            else
                username.AddText(Comment.LegacyName);

            if (Comment.EditedAt.HasValue)
            {
                info.Add(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.GetFont(size: 12),
                    Colour = colourProvider.Foreground1,
                    Text = $@"edited {HumanizerUtils.Humanize(Comment.EditedAt.Value)} by {Comment.EditedUser.Username}"
                });
            }

            if (Comment.HasMessage)
            {
                var formattedSource = MessageFormatter.FormatText(Comment.Message);
                message.AddLinks(formattedSource.Text, formattedSource.Links);
            }

            if (Comment.IsDeleted)
            {
                content.FadeColour(OsuColour.Gray(0.5f));
                votePill.Hide();
            }

            if (Comment.IsTopLevel)
            {
                AddInternal(new Box
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 1.5f,
                    Colour = OsuColour.Gray(0.1f)
                });
            }

            if (Replies.Any())
                onRepliesAdded(Replies);

            Replies.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        onRepliesAdded(args.NewItems.Cast<DrawableComment>());
                        break;

                    default:
                        throw new NotSupportedException(@"You can only add replies to this list. Other actions are not supported.");
                }
            };
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(show =>
            {
                if (Comment.IsDeleted)
                    this.FadeTo(show.NewValue ? 1 : 0);
            }, true);
            childrenExpanded.BindValueChanged(expanded => childCommentsVisibilityContainer.FadeTo(expanded.NewValue ? 1 : 0), true);

            updateButtonsState();

            base.LoadComplete();
        }

        private void onRepliesAdded(IEnumerable<DrawableComment> replies)
        {
            var page = createRepliesPage(replies);

            if (LoadState == LoadState.Loading)
            {
                addRepliesPage(page, replies);
                return;
            }

            LoadComponentAsync(page, loaded => addRepliesPage(loaded, replies));
        }

        public void AddNewReply(DrawableComment newReply)
        {
            LoadComponentAsync(newReply, loaded =>
            {
                loadedReplies.Add(loaded.Comment.Id, loaded.Comment);

                childCommentsContainer.Insert(-(childCommentsContainer.Count + 1), loaded);

                Comment.RepliesCount++;
                repliesButton.IncreaseValue();

                updateButtonsState();
            });
        }

        private void addRepliesPage(FillFlowContainer<DrawableComment> page, IEnumerable<DrawableComment> replies)
        {
            childCommentsContainer.Add(page);

            var newReplies = replies.Select(reply => reply.Comment);
            newReplies.ForEach(reply => loadedReplies.Add(reply.Id, reply));
            deletedCommentsCounter.Count.Value += newReplies.Count(reply => reply.IsDeleted);
            updateButtonsState();
        }

        private FillFlowContainer<DrawableComment> createRepliesPage(IEnumerable<DrawableComment> replies) => new FillFlowContainer<DrawableComment>
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = replies.ToList()
        };

        private void updateButtonsState()
        {
            var loadedReplesCount = loadedReplies.Count;
            var hasUnloadedReplies = loadedReplesCount != Comment.RepliesCount;

            loadMoreCommentsButton.FadeTo(hasUnloadedReplies && loadedReplesCount == 0 ? 1 : 0);
            showMoreButton.FadeTo(hasUnloadedReplies && loadedReplesCount > 0 ? 1 : 0);
            repliesButton.FadeTo(loadedReplesCount != 0 ? 1 : 0);

            if (Comment.IsTopLevel)
                chevronButton.FadeTo(loadedReplesCount != 0 ? 1 : 0);

            showMoreButton.IsLoading = loadMoreCommentsButton.IsLoading = false;
        }

        private class ChevronButton : ShowChildrenButton
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

        private class RepliesButton : ShowChildrenButton
        {
            private readonly SpriteText text;
            private int count;

            public RepliesButton(int count)
            {
                this.count = count;

                Child = text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                };
            }

            public void IncreaseValue()
            {
                count++;
                Expanded.TriggerChange();
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                text.Text = $@"{(expanded.NewValue ? "[+]" : "[-]")} replies ({count})";
            }
        }

        private class ReplyButton : CommentActionButton
        {
            public ReplyButton()
            {
                Child = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                    Text = @"reply"
                };
            }
        }

        private class LoadMoreCommentsButton : GetCommentRepliesButton
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Foreground1;
                HoverColour = Color4.White;
            }

            protected override string GetText() => @"[+] load replies";
        }

        private class ShowMoreButton : GetCommentRepliesButton
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Margin = new MarginPadding { Vertical = 10, Left = 80 };
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
            }

            protected override string GetText() => @"Show More";
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

        private class ReplyEditor : CancellableCommentEditor
        {
            public Action<CommentBundle> ReplyReceived;

            public readonly BindableBool IsVisible = new BindableBool();

            protected override string FooterText => @"Press Enter to reply.";

            protected override string CommitButtonText => @"Reply";

            protected override string TextBoxPlaceholder => @"Type your response here";

            [Resolved]
            private IAPIProvider api { get; set; }

            private PostCommentRequest request;

            private readonly Comment comment;

            public ReplyEditor(Comment comment)
            {
                this.comment = comment;
            }

            protected override void LoadComplete()
            {
                IsVisible.BindValueChanged(isVisible =>
                {
                    Alpha = isVisible.NewValue ? 1 : 0;
                }, true);

                OnCancel += () => IsVisible.Value = false;
                OnCommit += post;

                base.LoadComplete();
            }

            private void post(string message)
            {
                request = new PostCommentRequest(comment.CommentableId, comment.CommentableType, message, comment.Id);

                request.Success += response => Schedule(() =>
                {
                    ReplyReceived?.Invoke(response);

                    IsLoading = false;
                    IsVisible.Value = false;
                });

                api.Queue(request);
            }

            protected override void Dispose(bool isDisposing)
            {
                request?.Cancel();
                base.Dispose(isDisposing);
            }
        }
    }
}
