// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    public partial class SaveAndReloadEditorDialog : PopupDialog
    {
        public SaveAndReloadEditorDialog(Action reload, Action cancel)
        {
            HeaderText = EditorDialogsStrings.EditorReloadDialogHeader;

            Icon = FontAwesome.Solid.Sync;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = DialogStrings.Confirm,
                    Action = reload
                },
                new PopupDialogCancelButton
                {
                    Text = DialogStrings.Cancel,
                    Action = cancel
                }
            };
        }
    }
}
