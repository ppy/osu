// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class DownloadMissingVideosDialog : PopupDialog
    {
        public DownloadMissingVideosDialog(int count, Action confirmAction, Action? cancelAction = null)
        {
            HeaderText = MaintenanceSettingsStrings.ConfirmDownloadVideos(count);
            Icon = FontAwesome.Solid.Download;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = DialogStrings.Confirm,
                    Action = confirmAction
                },
                new PopupDialogCancelButton
                {
                    Text = DialogStrings.Cancel,
                    Action = cancelAction
                }
            };
        }
    }
}
