// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class ExitConfirmDialog : PopupDialog
    {
        public Action OnExit;
        public ExitConfirmDialog()
        {
            Icon = FontAwesome.fa_question;
            HeaderText = @"Confirm exit";
            BodyText = @"You'll left";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Totally.",
                    Action = delegate
                    {
                        OnExit?.Invoke();
                        Hide();
                    }
                },
                new PopupDialogCancelButton
                {
                    Text = @"Firetruck, I didn't mean to!",
                },
            };
        }
    }
}
