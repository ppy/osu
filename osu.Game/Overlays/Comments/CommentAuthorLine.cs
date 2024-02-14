// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
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

        private OsuSpriteText deletedLabel = null!;

        public CommentAuthorLine(Comment comment)
        {
            this.comment = comment;
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
