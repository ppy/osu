// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class DeleteAllBeatmapsDialog : PopupDialog
    {
        public DeleteAllBeatmapsDialog(Action deleteAction)
        {
            BodyText = "删除所有Σ(ﾟдﾟ;)?";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = @"确认删除";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"是的,gkd",
                    Action = deleteAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"不不不! 中止任务!",
                },
            };
        }
    }
}
