// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Dialog;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Input.Events;
using osu.Game.Audio.Effects;

namespace osu.Game.Overlays
{
    public partial class DialogOverlay : OsuFocusedOverlayContainer, IDialogOverlay
    {
        private readonly Container dialogContainer;

        protected override string PopInSampleName => "UI/dialog-pop-in";
        protected override string PopOutSampleName => "UI/dialog-pop-out";

        private AudioFilter lowPassFilter;

        public PopupDialog CurrentDialog { get; private set; }

        public DialogOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Child = dialogContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            };

            Width = 0.4f;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            AddInternal(lowPassFilter = new AudioFilter(audio.TrackMixer));
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
                CurrentDialog = null;
            }
        }

        public override bool IsPresent => Scheduler.HasPendingTasks || dialogContainer.Children.Count > 0;

        protected override bool BlockNonPositionalInput => true;

        protected override void PopIn()
        {
            lowPassFilter.CutoffTo(300, 100, Easing.OutCubic);
        }

        protected override void PopOut()
        {
            base.PopOut();
            lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 100, Easing.InCubic);

            // PopOut gets called initially, but we only want to hide dialog when we have been loaded and are present.
            if (IsLoaded && CurrentDialog?.State.Value == Visibility.Visible)
                CurrentDialog.Hide();
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
