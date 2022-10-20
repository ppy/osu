// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public class ReportCommentPopover : OsuPopover
    {
        private readonly DrawableComment comment;
        private OsuEnumDropdown<CommentReportReason> reason = null!;
        private readonly Bindable<string> commentText = new Bindable<string>();
        private RoundedButton submitButton = null!;

        public ReportCommentPopover(DrawableComment comment)
        {
            this.comment = comment;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = new ReverseChildIDFillFlowContainer<Drawable>
            {
                Direction = FillDirection.Vertical,
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(7),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = ReportStrings.CommentTitle(comment.Comment.User?.Username ?? comment.Comment.LegacyName!),
                        Font = OsuFont.Torus.With(size: 25),
                        Margin = new MarginPadding { Bottom = 10 }
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = UsersStrings.ReportReason,
                        Font = OsuFont.Torus.With(size: 20),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Child = reason = new OsuEnumDropdown<CommentReportReason>
                        {
                            RelativeSizeAxes = Axes.X
                        }
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = UsersStrings.ReportComments,
                        Font = OsuFont.Torus.With(size: 20),
                    },
                    new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        PlaceholderText = UsersStrings.ReportPlaceholder,
                        Current = commentText
                    },
                    submitButton = new RoundedButton
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Width = 200,
                        BackgroundColour = colours.Red3,
                        Text = UsersStrings.ReportActionsSend,
                        Action = send,
                        Margin = new MarginPadding { Bottom = 5, Top = 10 },
                    }
                }
            };
            commentText.BindValueChanged(e =>
            {
                submitButton.Enabled.Value = !string.IsNullOrWhiteSpace(e.NewValue);
            }, true);
        }

        private void send()
        {
            if (string.IsNullOrWhiteSpace(commentText.Value))
                return;

            this.HidePopover();
            comment.ReportComment(reason.Current.Value, commentText.Value);
        }
    }
}
