// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Dialog;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class DialogOverlay : OsuFocusedOverlayContainer, IDialogOverlay
    {
        private readonly Box dimLayer;
        private readonly Container dialogContainer;

        protected override string PopInSampleName => "UI/dialog-pop-in";
        protected override string PopOutSampleName => "UI/dialog-pop-out";

        protected override bool DimMainContent => false;

        [Resolved]
        private MusicController musicController { get; set; }

        public PopupDialog CurrentDialog { get; private set; }

        public override bool IsPresent => (Scheduler.HasPendingTasks || dialogContainer.Children.Count > 0)
                                          // The following line ensures that dialogs are not presented while the dialog overlay
                                          // cannot be displayed. This is due to the `Schedule` usage inside `Push()`.
                                          //
                                          // Without this, a dialog pushed during disabled overlay activation mode would be presented,
                                          // but immediately dismissed without ever being seen by the user (see
                                          // https://github.com/ppy/osu/blob/ce5e54c9d27b17d460d99e774de502f9480fb710/osu.Game/Graphics/Containers/OsuFocusedOverlayContainer.cs#L131-L136).
                                          && OverlayActivationMode.Value != OverlayActivation.Disabled;

        [CanBeNull]
        private IDisposable duckOperation;

        public DialogOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    dimLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f,
                    },
                    dialogContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Y,
                        Width = 500,
                    },
                },
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            duckOperation?.Dispose();
        }

        public void Push(PopupDialog dialog)
        {
            if (dialog == CurrentDialog || dialog.State.Value == Visibility.Hidden) return;

            // Immediately update the externally accessible property as this may be used for checks even before
            // a DialogOverlay instance has finished loading.
            var lastDialog = CurrentDialog;
            CurrentDialog = dialog;

            Schedule(() =>
            {
                // if any existing dialog is being displayed, dismiss it before showing a new one.
                lastDialog?.Hide();

                // if the new dialog is hidden before added to the dialogContainer, bypass any further operations.
                if (dialog.State.Value == Visibility.Hidden)
                {
                    dismiss();
                    return;
                }

                Logger.Log($"{nameof(DialogOverlay)}: Showing dialog {dialog}");
                dialogContainer.Add(dialog);
                Show();

                dialog.State.BindValueChanged(state =>
                {
                    if (state.NewValue != Visibility.Hidden) return;

                    // Trigger the demise of the dialog as soon as it hides.
                    dialog.Delay(PopupDialog.EXIT_DURATION).Expire();

                    dismiss();
                });
            });

            void dismiss()
            {
                if (dialog != CurrentDialog) return;

                // Handle the case where the dialog is the currently displayed dialog.
                // In this scenario, the overlay itself should also be hidden.
                Hide();
                Logger.Log($"{nameof(DialogOverlay)}: Dismissing dialog {dialog}");
                CurrentDialog = null;
            }
        }

        protected override bool BlockNonPositionalInput => true;

        private bool closeOnMouseUp;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            closeOnMouseUp = !dialogContainer.ReceivePositionalInputAt(e.ScreenSpaceMousePosition);

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (closeOnMouseUp && !dialogContainer.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                Hide();

            base.OnMouseUp(e);
        }

        protected override void PopIn()
        {
            duckOperation = musicController?.Duck(new DuckParameters
            {
                DuckVolumeTo = 1,
                DuckDuration = 100,
                RestoreDuration = 100,
            });

            dimLayer.FadeTo(0.5f, PopupDialog.ENTER_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();
            duckOperation?.Dispose();

            // PopOut gets called initially, but we only want to hide dialog when we have been loaded and are present.
            if (IsLoaded && CurrentDialog?.State.Value == Visibility.Visible)
                CurrentDialog.Hide();

            dimLayer.FadeOut(PopupDialog.EXIT_DURATION, Easing.OutQuint);
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    var clickableButton =
                        CurrentDialog?.Buttons.OfType<PopupDialogOkButton>().FirstOrDefault() ??
                        CurrentDialog?.Buttons.First();

                    clickableButton?.TriggerClick();
                    return true;
            }

            return base.OnPressed(e);
        }
    }
}
