// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class DeleteDifficultyConfirmationDialog : DeletionDialog
    {
        public DeleteDifficultyConfirmationDialog(string difficultyName, int objectCount, Action deleteAction)
        {
            BodyText = EditorDialogsStrings.DeleteDifficultyDetails(difficultyName, objectCount);
            DangerousAction = deleteAction;
        }
    }
}
