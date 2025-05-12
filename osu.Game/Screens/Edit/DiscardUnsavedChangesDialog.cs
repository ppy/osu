// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class DiscardUnsavedChangesDialog : PopupDialog
    {
        public DiscardUnsavedChangesDialog(Action exit)
        {
            HeaderText = EditorDialogsStrings.DiscardUnsavedChangesDialogHeader;

            Icon = FontAwesome.Solid.Trash;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = EditorDialogsStrings.ForgetAllChanges,
                    Action = exit
                },
                new PopupDialogCancelButton
                {
                    Text = EditorDialogsStrings.ContinueEditing,
                },
            };
        }
    }
}
