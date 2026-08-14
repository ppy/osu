// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A generic dialog for sending an online report about something.
    /// </summary>
    /// <typeparam name="TReportReason">An enumeration type with all valid reasons for the report.</typeparam>
    public abstract partial class ReportDialog<TReportReason> : PopupDialog
        where TReportReason : struct, Enum
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        /// <summary>
        /// Intentionally hardcoded to `OverlayColourScheme.Plum` since it fits well
        /// with the colour scheme of the `PopupDialog`.
        /// </summary>
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        /// <summary>
        /// The action to run when the report is submitted.
        /// </summary>
        public Action? Submitted { get; set; }

        /// <summary>
        /// The action to run when the report is submitted successfully.
        /// </summary>
        public Action? Success { get; set; }

        /// <summary>
        /// The action to run when the report failed to submit.
        /// </summary>
        public Action? Failure { get; set; }

        private readonly ReverseChildIDFillFlowContainer<Drawable> form;
        private readonly FormEnumDropdown<TReportReason> reasonDropdown;
        private readonly FormTextBox commentsTextBox;
        private readonly SettingsNote errorNote;
        private readonly PopupDialogButton submitButton;
        private readonly LoadingLayer loadingLayer;

        private readonly bool showConfirmation;

        /// <summary>
        /// Creates a new <see cref="ReportDialog{TReportReason}"/>.
        /// </summary>
        /// <param name="headerString">The text to display in the header of the dialog.</param>
        /// <param name="showConfirmation">
        /// Whether the dialog should show a generic "Thank you for your report" confirmation message.
        /// Set this to <c>false</c> if you're displaying a custom message outside of this dialog.
        /// </param>
        protected ReportDialog(LocalisableString headerString, bool showConfirmation = true)
        {
            this.showConfirmation = showConfirmation;

            Icon = FontAwesome.Solid.ExclamationTriangle;
            HeaderText = headerString;

            MainContent.Add(form = new ReverseChildIDFillFlowContainer<Drawable>
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(7),
                Padding = new MarginPadding { Horizontal = 20 },
                Children = new Drawable[]
                {
                    reasonDropdown = new FormEnumDropdown<TReportReason>
                    {
                        Caption = UsersStrings.ReportReason,
                    },
                    commentsTextBox = new FormTextBox
                    {
                        Caption = UsersStrings.ReportComments,
                        PlaceholderText = UsersStrings.ReportPlaceholder,
                    },
                    errorNote = new SettingsNote
                    {
                        RelativeSizeAxes = Axes.X,
                    },
                },
            });

            Add(loadingLayer = new LoadingLayer(true)
            {
                RelativeSizeAxes = Axes.Both,
            });

            Buttons = new[]
            {
                submitButton = new SubmitButton
                {
                    Action = () =>
                    {
                        if (showConfirmation)
                            loadingLayer.Show();

                        // we don't want size easing to mess up any transforms that are happening
                        // when the dialog is appearing, hence easing is only enabled after
                        // the report is submitted
                        Content.AutoSizeEasing = Easing.OutQuint;
                        Content.AutoSizeDuration = 500F;

                        Submitted?.Invoke();
                        performRequest();

                        if (!showConfirmation)
                            Hide();
                    }
                },
                new PopupDialogCancelButton { Text = WebCommonStrings.ButtonsCancel },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            commentsTextBox.Current.BindValueChanged(_ => updateStatus());

            reasonDropdown.Current.BindValueChanged(_ => updateStatus());

            updateStatus();
        }

        private void performRequest()
        {
            var request = GetRequest(reasonDropdown.Current.Value, commentsTextBox.Current.Value);

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
                    Buttons = [];
                    HeaderText = UsersStrings.ReportThanks;

                    loadingLayer.Hide();
                    Scheduler.AddDelayed(Hide, 2000);
                });
            }

            Success?.Invoke();
        }

        private void handleFailure(Exception e)
        {
            if (showConfirmation)
            {
                Schedule(() => errorNote.Current.Value = new SettingsNote.Data(e.Message, SettingsNote.Type.Critical));
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

        public partial class SubmitButton : PopupDialogButton
        {
            public override bool HideDialogBeforeInvoke => false;

            public SubmitButton()
                : base(HoverSampleSet.DialogOk)
            {
                Text = UsersStrings.ReportActionsSend;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                ButtonColour = colours.Red;
            }
        }
    }
}
