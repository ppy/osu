﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        private PopupDialog currentDialog;

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
            if (dialog == currentDialog) return;

            currentDialog?.Hide();
            currentDialog = dialog;

            dialogContainer.Add(currentDialog);

            currentDialog.Show();
            currentDialog.State.ValueChanged += state => onDialogOnStateChanged(dialog, state.NewValue);
            Show();
        }

        protected override bool BlockNonPositionalInput => true;

        private void onDialogOnStateChanged(VisibilityContainer dialog, Visibility v)
        {
            if (v != Visibility.Hidden) return;

            //handle the dialog being dismissed.
            dialog.Delay(PopupDialog.EXIT_DURATION).Expire();

            if (dialog == currentDialog)
                Hide();
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(PopupDialog.ENTER_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (currentDialog?.State.Value == Visibility.Visible)
            {
                currentDialog.Hide();
                return;
            }

            this.FadeOut(PopupDialog.EXIT_DURATION, Easing.InSine);
        }

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Select:
                    currentDialog?.Buttons.OfType<PopupDialogOkButton>().FirstOrDefault()?.Click();
                    return true;
            }

            return base.OnPressed(action);
        }
    }
}
