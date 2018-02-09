// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Dialog;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public class DialogOverlay : OsuFocusedOverlayContainer
    {
        private readonly Container dialogContainer;
        private PopupDialog currentDialog;

        public void Push(PopupDialog dialog)
        {
            if (dialog == currentDialog) return;

            currentDialog?.Hide();
            currentDialog = dialog;

            dialogContainer.Add(currentDialog);

            currentDialog.Show();
            currentDialog.StateChanged += state => onDialogOnStateChanged(dialog, state);
            State = Visibility.Visible;
        }

        private void onDialogOnStateChanged(VisibilityContainer dialog, Visibility v)
        {
            if (v != Visibility.Hidden) return;

            //handle the dialog being dismissed.
            dialog.Delay(PopupDialog.EXIT_DURATION).Expire();

            if (dialog == currentDialog)
                State = Visibility.Hidden;
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(PopupDialog.ENTER_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(PopupDialog.EXIT_DURATION, Easing.InSine);
        }

        public DialogOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                    },
                },
                dialogContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}
