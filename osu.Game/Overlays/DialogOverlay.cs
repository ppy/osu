// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Dialog;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using System.Linq;

namespace osu.Game.Overlays
{
    public class DialogOverlay : OsuFocusedOverlayContainer
    {
        private readonly Container dialogContainer;

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

        public void Push(PopupDialog dialog)
        {
            if (dialog == CurrentDialog) return;

            CurrentDialog?.Hide();
            CurrentDialog = dialog;

            dialogContainer.Add(CurrentDialog);

            CurrentDialog.Show();
            CurrentDialog.State.ValueChanged += state => onDialogOnStateChanged(dialog, state.NewValue);
            Show();
        }

        protected override bool BlockNonPositionalInput => true;

        private void onDialogOnStateChanged(VisibilityContainer dialog, Visibility v)
        {
            if (v != Visibility.Hidden) return;

            //handle the dialog being dismissed.
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
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (CurrentDialog?.State.Value == Visibility.Visible)
            {
                CurrentDialog.Hide();
                return;
            }

            this.FadeOut(PopupDialog.EXIT_DURATION, Easing.InSine);
        }

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Select:
                    CurrentDialog?.Buttons.OfType<PopupDialogOkButton>().FirstOrDefault()?.Click();
                    return true;
            }

            return base.OnPressed(action);
        }
    }
}
