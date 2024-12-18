// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A generic popover for sending an online report about something.
    /// </summary>
    /// <typeparam name="TReportReason">An enumeration type with all valid reasons for the report.</typeparam>
    public abstract partial class ReportPopover<TReportReason> : OsuPopover
        where TReportReason : struct, Enum
    {
        /// <summary>
        /// The action to run when the report is finalised.
        /// The arguments to this action are: the reason for the report, and an optional additional comment.
        /// </summary>
        public Action<TReportReason, string>? Action;

        private OsuEnumDropdown<TReportReason> reasonDropdown = null!;
        private OsuTextBox commentsTextBox = null!;
        private RoundedButton submitButton = null!;

        private readonly LocalisableString header;

        /// <summary>
        /// Creates a new <see cref="ReportPopover{TReportReason}"/>.
        /// </summary>
        /// <param name="headerString">The text to display in the header of the popover.</param>
        protected ReportPopover(LocalisableString headerString)
        {
            header = headerString;
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
                    new SpriteIcon
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Icon = FontAwesome.Solid.ExclamationTriangle,
                        Size = new Vector2(36),
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = header,
                        Font = OsuFont.Torus.With(size: 25),
                        Margin = new MarginPadding { Bottom = 10 }
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = UsersStrings.ReportReason,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Child = reasonDropdown = new OsuEnumDropdown<TReportReason>
                        {
                            RelativeSizeAxes = Axes.X
                        }
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = UsersStrings.ReportComments,
                    },
                    commentsTextBox = new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        PlaceholderText = UsersStrings.ReportPlaceholder,
                    },
                    submitButton = new RoundedButton
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Width = 200,
                        BackgroundColour = colours.Red3,
                        Text = UsersStrings.ReportActionsSend,
                        Action = () =>
                        {
                            Action?.Invoke(reasonDropdown.Current.Value, commentsTextBox.Text);
                            this.HidePopover();
                        },
                        Margin = new MarginPadding { Bottom = 5, Top = 10 },
                    }
                }
            };

            commentsTextBox.Current.BindValueChanged(_ => updateStatus());

            reasonDropdown.Current.BindValueChanged(_ => updateStatus());

            updateStatus();
        }

        private void updateStatus()
        {
            submitButton.Enabled.Value = !string.IsNullOrWhiteSpace(commentsTextBox.Current.Value) || !IsCommentRequired(reasonDropdown.Current.Value);
        }

        /// <summary>
        /// Determines whether an additional comment is required for submitting the report with the supplied <paramref name="reason"/>.
        /// </summary>
        protected virtual bool IsCommentRequired(TReportReason reason) => true;
    }
}
