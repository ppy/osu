// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public partial class CommentAuthorLine : FillFlowContainer
    {
        private readonly Comment comment;
        private readonly IReadOnlyList<CommentableMeta> meta;

        private OsuSpriteText deletedLabel = null!;

        public CommentAuthorLine(Comment comment, IReadOnlyList<CommentableMeta> meta)
        {
            this.comment = comment;
            this.meta = meta;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(4, 0);

            Add(new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold))
            {
                AutoSizeAxes = Axes.Both
            }.With(username =>
            {
                if (comment.UserId.HasValue)
                    username.AddUserLink(comment.User);
                else
                    username.AddText(comment.LegacyName!);
            }));

            var ownerMeta = meta.FirstOrDefault(m => m.Id == comment.CommentableId && m.Type == comment.CommentableType);

            if (ownerMeta?.OwnerId != null && ownerMeta.OwnerId == comment.UserId)
            {
                Add(new OwnerTitleBadge(ownerMeta.OwnerTitle ?? string.Empty)
                {
                    // add top space to align with username
                    Margin = new MarginPadding { Top = 1f },
                });
            }

            if (comment.Pinned)
                Add(new PinnedCommentNotice());

            Add(new ParentUsername(comment));

            Add(deletedLabel = new OsuSpriteText
            {
                Alpha = 0f,
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                Text = CommentsStrings.Deleted
            });
        }

        public void MarkDeleted()
        {
            deletedLabel.Show();
        }

        private partial class OwnerTitleBadge : CircularContainer
        {
            private readonly string title;

            public OwnerTitleBadge(string title)
            {
                this.title = title;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Light1,
                    },
                    new OsuSpriteText
                    {
                        Text = title,
                        Font = OsuFont.Default.With(size: 10, weight: FontWeight.Bold),
                        Margin = new MarginPadding { Vertical = 2, Horizontal = 5 },
                        Colour = colourProvider.Background6,
                    },
                };
            }
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
