// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ConfirmBeatmapAudioReplaceDialog : PopupDialog
    {
        public ConfirmBeatmapAudioReplaceDialog(Action replaceAudio, Action keepExistingAudio)
        {
            HeaderText = EditorDialogsStrings.BeatmapAudioReplaceDialogHeader;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = EditorDialogsStrings.ReplaceAudio,
                    Action = replaceAudio
                },
                new PopupDialogCancelButton
                {
                    Text = EditorDialogsStrings.KeepExistingAudio,
                    Action = keepExistingAudio
                },
            };
        }
    }
}
