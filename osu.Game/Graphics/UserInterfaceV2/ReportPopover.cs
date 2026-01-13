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
using osu.Game.Online.API;
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
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        /// <summary>
        /// The action to run when the report is submitted.
        /// </summary>
        public event Action? Submitted;

        /// <summary>
        /// The action to run when the report is submitted successfully.
        /// </summary>
        public event Action? Success;

        /// <summary>
        /// The action to run when the report failed to submit.
        /// </summary>
        public event Action? Failure;

        private ReverseChildIDFillFlowContainer<Drawable> form = null!;
        private ReportConfirmation confirmation = null!;
        private OsuEnumDropdown<TReportReason> reasonDropdown = null!;
        private OsuTextBox commentsTextBox = null!;
        private ErrorTextFlowContainer errorMessage = null!;
        private RoundedButton submitButton = null!;
        private LoadingLayer loadingLayer = null!;

        private readonly LocalisableString header;

        private readonly bool showConfirmation;

        /// <summary>
        /// Creates a new <see cref="ReportPopover{TReportReason}"/>.
        /// </summary>
        /// <param name="headerString">The text to display in the header of the popover.</param>
        /// <param name="showConfirmation">
        /// Whether the popover should show a generic "Thank you for your report" confirmation message.
        /// Set this to `true` if you're displaying a custom message outside of this popover.
        /// </param>
        protected ReportPopover(LocalisableString headerString, bool showConfirmation = true)
            : base(false)
        {
            header = headerString;
            this.showConfirmation = showConfirmation;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.AutoSizeAxes = Axes.Y;
            Content.Width = 500;

            Children = new Drawable[]
            {
                form = new ReverseChildIDFillFlowContainer<Drawable>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(7),
                    Padding = new MarginPadding(20),
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
                        errorMessage = new ErrorTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
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
                                if (showConfirmation)
                                    loadingLayer.Show();

                                // we don't want size easing to mess up any transforms that are happening
                                // when the popover is appearing, hence easing is only enabled after
                                // the report is submitted
                                Content.AutoSizeEasing = Easing.OutQuint;
                                Content.AutoSizeDuration = 500F;

                                Submitted?.Invoke();
                                performRequest();

                                if (!showConfirmation)
                                    this.HidePopover();
                            },
                            Margin = new MarginPadding { Bottom = 5, Top = 10 },
                        },
                    },
                },
                confirmation = new ReportConfirmation(),
                loadingLayer = new LoadingLayer(true)
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };

            commentsTextBox.Current.BindValueChanged(_ => updateStatus());

            reasonDropdown.Current.BindValueChanged(_ => updateStatus());

            updateStatus();
        }

        private void performRequest()
        {
            var request = GetRequest(reasonDropdown.Current.Value, commentsTextBox.Text);

            request.Success += handleSuccess;
            request.Failure += handleFailure;

            api.Queue(request);
        }

        private void handleSuccess()
        {
            if (showConfirmation)
            {
                Schedule(() =>
                {
                    form.Hide();
                    confirmation.Show();

                    loadingLayer.Hide();
                    Scheduler.AddDelayed(this.HidePopover, 2000);
                });
            }

            Success?.Invoke();
        }

        private void handleFailure(Exception e)
        {
            if (showConfirmation)
            {
                Schedule(() => errorMessage.AddErrors([e.Message]));
                loadingLayer.Hide();
            }

            Failure?.Invoke();
        }

        private void updateStatus()
        {
            submitButton.Enabled.Value = !string.IsNullOrWhiteSpace(commentsTextBox.Current.Value) || !IsCommentRequired(reasonDropdown.Current.Value);
        }

        /// <summary>
        /// Returns the API request responsible for submitting this report.
        /// </summary>
        /// <param name="reason">The reason for this report.</param>
        /// <param name="comments">An optional comment explaining the report.</param>
        /// <returns></returns>
        protected abstract APIRequest GetRequest(TReportReason reason, string comments);

        /// <summary>
        /// Determines whether an additional comment is required for submitting the report with the supplied <paramref name="reason"/>.
        /// </summary>
        protected virtual bool IsCommentRequired(TReportReason reason) => true;

        public partial class ReportConfirmation : FillFlowContainer
        {
            public ReportConfirmation()
            {
                Direction = FillDirection.Vertical;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Spacing = new Vector2(7);
                Padding = new MarginPadding(20);
                Alpha = 0;

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
                        Text = UsersStrings.ReportThanks,
                        Font = OsuFont.Torus.With(size: 25),
                        Margin = new MarginPadding { Bottom = 10 }
                    },
                };
            }
        }
    }
}
