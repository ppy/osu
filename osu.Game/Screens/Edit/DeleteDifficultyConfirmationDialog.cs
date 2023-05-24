// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class DeleteDifficultyConfirmationDialog : DangerousActionDialog
    {
        public DeleteDifficultyConfirmationDialog(BeatmapInfo beatmapInfo, Action deleteAction)
        {
            BodyText = $"\"{beatmapInfo.DifficultyName}\" difficulty";
            DangerousAction = deleteAction;
        }
    }
}
