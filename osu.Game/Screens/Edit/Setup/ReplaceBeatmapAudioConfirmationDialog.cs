// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class ReplaceBeatmapAudioConfirmationDialog : DangerousActionDialog
    {
        public ReplaceBeatmapAudioConfirmationDialog(Action replaceAudio, Action keepExistingAudio)
        {
            BodyText = EditorDialogsStrings.ReplaceBeatmapAudioDialogBody;
            DangerousAction = replaceAudio;
            CancelAction = keepExistingAudio;
        }
    }
}
