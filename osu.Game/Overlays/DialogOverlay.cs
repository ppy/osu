// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class DialogOverlay : OsuFocusedOverlayContainer
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
            if (dialog == CurrentDialog || dialog.State.Value != Visibility.Visible) return;

            // if any existing dialog is being displayed, dismiss it before showing a new one.
            CurrentDialog?.Hide();

            CurrentDialog = dialog;
            CurrentDialog.State.ValueChanged += state => onDialogOnStateChanged(dialog, state.NewValue);

            dialogContainer.Add(CurrentDialog);

            Show();
        }

        public override bool IsPresent => dialogContainer.Children.Count > 0;

        protected override bool BlockNonPositionalInput => true;

        private void onDialogOnStateChanged(VisibilityContainer dialog, Visibility v)
        {
            if (v != Visibility.Hidden) return;

            // handle the dialog being dismissed.
            dialog.Delay(PopupDialog.EXIT_DURATION).Expire();

            if (dialog == CurrentDialog)
            {
                Hide();
                CurrentDialog = null;
            }
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(PopupDialog.ENTER_DURATION, Easing.OutQuint);
            lowPassFilter.CutoffTo(300, 100, Easing.OutCubic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 100, Easing.InCubic);

            if (CurrentDialog?.State.Value == Visibility.Visible)
            {
                CurrentDialog.Hide();
                return;
            }

            this.FadeOut(PopupDialog.EXIT_DURATION, Easing.InSine);
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    CurrentDialog?.Buttons.OfType<PopupDialogOkButton>().FirstOrDefault()?.TriggerClick();
                    return true;
            }

            return base.OnPressed(e);
        }
    }
}
