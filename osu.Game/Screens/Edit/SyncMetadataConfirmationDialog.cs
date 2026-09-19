// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Edit
{
    public partial class SyncMetadataConfirmationDialog : DangerousActionDialog
    {
        public SyncMetadataConfirmationDialog(Action syncAction)
        {
            BodyText = EditorDialogsStrings.SyncMetadataConfirmationBody;
            DangerousAction = syncAction;
        }
    }
}
