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
using osu.Framework.Graphics.Cursor;
using osu.Framework.Bindables;
using System.Linq;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using System.Collections.Generic;
using System;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.IEnumerableExtensions;
using System.Collections.Specialized;
using System.Diagnostics;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Comments.Buttons;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.OSD;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    [Cached]
    public partial class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;

        public Action<DrawableComment, int> RepliesRequested = null!;

        public readonly Comment Comment;

        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        private readonly Dictionary<long, Comment> loadedReplies = new Dictionary<long, Comment>();

        public readonly BindableList<DrawableComment> Replies = new BindableList<DrawableComment>();

        private readonly BindableBool childrenExpanded = new BindableBool(true);

        private int currentPage;

        /// <summary>
        /// Local field for tracking comment state. Initialized from Comment.IsDeleted, may change when deleting was requested by user.
        /// </summary>
        public bool WasDeleted { get; protected set; }

        /// <summary>
        /// Tracks this comment's level of nesting. 0 means that this comment has no parents.
        /// </summary>
        public int Level { get; private set; }

        private FillFlowContainer childCommentsVisibilityContainer = null!;
        private FillFlowContainer childCommentsContainer = null!;
        private LoadRepliesButton loadRepliesButton = null!;
        private ShowMoreRepliesButton showMoreButton = null!;
        private ShowRepliesButton showRepliesButton = null!;
        private ChevronButton chevronButton = null!;
        private LinkFlowContainer actionsContainer = null!;
        private LoadingSpinner actionsLoading = null!;
        private DeletedCommentsCounter deletedCommentsCounter = null!;
        private OsuSpriteText deletedLabel = null!;
        private GridContainer content = null!;
        private VotePill votePill = null!;
        private Container<CommentEditor> replyEditorContainer = null!;
        private Container repliesButtonContainer = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private Clipboard clipboard { get; set; } = null!;

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        public DrawableComment(Comment comment)
        {
            Comment = comment;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, DrawableComment? parentComment)
        {
            LinkFlowContainer username;
            FillFlowContainer info;
            CommentMarkdownContainer message;

            Level = parentComment?.Level + 1 ?? 0;

            float childrenPadding = Level < 6 ? 20 : 5;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = getPadding(Comment.IsTopLevel),
                    Child = new FillFlowContainer
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
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, size: avatar_size + 10),
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
                                        new Container
                                        {
                                            Size = new Vector2(avatar_size),
                                            Children = new Drawable[]
                                            {
                                                new UpdateableAvatar(Comment.User)
                                                {
                                                    Size = new Vector2(avatar_size),
                                                    Masking = true,
                                                    CornerRadius = avatar_size / 2f,
                                                    CornerExponent = 2,
                                                },
                                                votePill = new VotePill(Comment)
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreRight,
                                                    Margin = new MarginPadding
                                                    {
                                                        Right = 5
                                                    }
                                                }
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 4),
                                            Margin = new MarginPadding
                                            {
                                                Vertical = 2
                                            },
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10, 0),
                                                    Children = new[]
                                                    {
                                                        username = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold))
                                                        {
                                                            AutoSizeAxes = Axes.Both
                                                        },
                                                        Comment.Pinned ? new PinnedCommentNotice() : Empty(),
                                                        new ParentUsername(Comment),
                                                        deletedLabel = new OsuSpriteText
                                                        {
                                                            Alpha = 0f,
                                                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                                                            Text = CommentsStrings.Deleted
                                                        }
                                                    }
                                                },
                                                message = new CommentMarkdownContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    DocumentMargin = new MarginPadding(0),
                                                    DocumentPadding = new MarginPadding(0),
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10, 0),
                                                    Children = new Drawable[]
                                                    {
                                                        info = new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(10, 0),
                                                            Children = new Drawable[]
                                                            {
                                                                new DrawableDate(Comment.CreatedAt, 12, false)
                                                                {
                                                                    Colour = colourProvider.Foreground1
                                                                }
                                                            }
                                                        },
                                                        actionsContainer = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold))
                                                        {
                                                            Name = @"Actions buttons",
                                                            AutoSizeAxes = Axes.Both,
                                                        },
                                                        actionsLoading = new LoadingSpinner
                                                        {
                                                            Size = new Vector2(12f),
                                                            Anchor = Anchor.TopLeft,
                                                            Origin = Anchor.TopLeft
                                                        }
                                                    }
                                                },
                                                replyEditorContainer = new Container<CommentEditor>
                                                {
                                                    AutoSizeAxes = Axes.Y,
                                                    RelativeSizeAxes = Axes.X,
                                                    Padding = new MarginPadding { Top = 10 },
                                                    Alpha = 0,
                                                },
                                                repliesButtonContainer = new Container
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Alpha = 0,
                                                    Children = new Drawable[]
                                                    {
                                                        showRepliesButton = new ShowRepliesButton(Comment.RepliesCount)
                                                        {
                                                            Expanded = { BindTarget = childrenExpanded }
                                                        },
                                                        loadRepliesButton = new LoadRepliesButton
                                                        {
                                                            Action = () => RepliesRequested(this, ++currentPage)
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
                                Name = @"Children comments",
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding { Left = childrenPadding },
                                Children = new Drawable[]
                                {
                                    childCommentsContainer = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical
                                    },
                                    deletedCommentsCounter = new DeletedCommentsCounter
                                    {
                                        ShowDeleted = { BindTarget = ShowDeleted },
                                        Margin = new MarginPadding
                                        {
                                            Top = 10
                                        }
                                    },
                                    showMoreButton = new ShowMoreRepliesButton
                                    {
                                        Action = () => RepliesRequested(this, ++currentPage)
                                    }
                                }
                            },
                        }
                    }
                },
                new Container
                {
                    Size = new Vector2(70, 40),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Horizontal = 5 },
                    Child = chevronButton = new ChevronButton
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Expanded = { BindTarget = childrenExpanded },
                        Alpha = 0
                    }
                }
            };

            if (Comment.UserId.HasValue)
                username.AddUserLink(Comment.User);
            else
                username.AddText(Comment.LegacyName!);

            if (Comment.EditedAt.HasValue && Comment.EditedUser != null)
            {
                var font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular);
                var colour = colourProvider.Foreground1;

                info.Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = font,
                            Text = "edited ",
                            Colour = colour
                        },
                        new DrawableDate(Comment.EditedAt.Value)
                        {
                            Font = font,
                            Colour = colour
                        },
                        new OsuSpriteText
                        {
                            Font = font,
                            Text = $@" by {Comment.EditedUser.Username}",
                            Colour = colour
                        },
                    }
                });
            }

            if (Comment.HasMessage)
                message.Text = Comment.Message;

            WasDeleted = Comment.IsDeleted;
            if (WasDeleted)
                makeDeleted();

            actionsContainer.AddLink(CommonStrings.ButtonsPermalink, copyUrl);
            actionsContainer.AddArbitraryDrawable(Empty().With(d => d.Width = 10));
            actionsContainer.AddLink(CommonStrings.ButtonsReply.ToLower(), toggleReply);
            actionsContainer.AddArbitraryDrawable(Empty().With(d => d.Width = 10));

            if (Comment.UserId.HasValue && Comment.UserId.Value == api.LocalUser.Value.Id)
                actionsContainer.AddLink(CommonStrings.ButtonsDelete.ToLower(), deleteComment);
            else
                actionsContainer.AddArbitraryDrawable(new CommentReportButton(Comment));

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
                        Debug.Assert(args.NewItems != null);

                        onRepliesAdded(args.NewItems.Cast<DrawableComment>());
                        break;

                    default:
                        throw new NotSupportedException(@"You can only add replies to this list. Other actions are not supported.");
                }
            };
        }

        /// <summary>
        /// Transforms some comment's components to show it as deleted. Invoked both from loading and deleting.
        /// </summary>
        private void makeDeleted()
        {
            deletedLabel.Show();
            content.FadeColour(OsuColour.Gray(0.5f));
            votePill.Hide();
            actionsContainer.Expire();
        }

        /// <summary>
        /// Invokes comment deletion with confirmation.
        /// </summary>
        private void deleteComment()
        {
            if (dialogOverlay == null)
                deleteCommentRequest();
            else
                dialogOverlay.Push(new ConfirmDialog("Do you really want to delete your comment?", deleteCommentRequest));
        }

        /// <summary>
        /// Invokes comment deletion directly.
        /// </summary>
        private void deleteCommentRequest()
        {
            actionsContainer.Hide();
            actionsLoading.Show();
            var request = new CommentDeleteRequest(Comment.Id);
            request.Success += _ => Schedule(() =>
            {
                actionsLoading.Hide();
                makeDeleted();
                WasDeleted = true;
                if (!ShowDeleted.Value)
                    Hide();
            });
            request.Failure += e => Schedule(() =>
            {
                Logger.Error(e, "Failed to delete comment");
                actionsLoading.Hide();
                actionsContainer.Show();
            });
            api.Queue(request);
        }

        private void copyUrl()
        {
            clipboard.SetText($@"{api.APIEndpointUrl}/comments/{Comment.Id}");
            onScreenDisplay?.Display(new CopyUrlToast());
        }

        private void toggleReply()
        {
            if (replyEditorContainer.Count == 0)
            {
                replyEditorContainer.Show();
                replyEditorContainer.Add(new ReplyCommentEditor(Comment)
                {
                    OnPost = comments =>
                    {
                        Comment.RepliesCount += comments.Length;
                        showRepliesButton.Count = Comment.RepliesCount;
                        Replies.AddRange(comments);
                    },
                    OnCancel = toggleReply
                });
            }
            else
            {
                replyEditorContainer.ForEach(e => e.Expire());
                replyEditorContainer.Hide();
            }
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(show =>
            {
                if (WasDeleted)
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
            int loadedRepliesCount = loadedReplies.Count;
            bool hasUnloadedReplies = loadedRepliesCount != Comment.RepliesCount;

            showRepliesButton.FadeTo(loadedRepliesCount != 0 ? 1 : 0);
            loadRepliesButton.FadeTo(hasUnloadedReplies && loadedRepliesCount == 0 ? 1 : 0);
            repliesButtonContainer.FadeTo(repliesButtonContainer.Any(child => child.Alpha > 0) ? 1 : 0);

            showMoreButton.FadeTo(hasUnloadedReplies && loadedRepliesCount > 0 ? 1 : 0);

            if (Comment.IsTopLevel)
                chevronButton.FadeTo(loadedRepliesCount != 0 ? 1 : 0);

            showMoreButton.IsLoading = loadRepliesButton.IsLoading = false;
        }

        private MarginPadding getPadding(bool isTopLevel)
        {
            if (isTopLevel)
            {
                return new MarginPadding
                {
                    Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING,
                    Vertical = 15
                };
            }

            return new MarginPadding
            {
                Top = 10
            };
        }

        private partial class PinnedCommentNotice : FillFlowContainer
        {
            public PinnedCommentNotice()
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(2, 0);
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Thumbtack,
                        Size = new Vector2(14),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                        Text = CommentsStrings.Pinned,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    }
                };
            }
        }

        private partial class ParentUsername : FillFlowContainer, IHasTooltip
        {
            public LocalisableString TooltipText => getParentMessage();

            private readonly Comment? parentComment;

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
                        Text = parentComment?.User?.Username ?? parentComment?.LegacyName!
                    }
                };
            }

            private LocalisableString getParentMessage()
            {
                if (parentComment == null)
                    return string.Empty;

                return parentComment.HasMessage ? parentComment.Message : parentComment.IsDeleted ? CommentsStrings.Deleted : string.Empty;
            }
        }
    }
}
