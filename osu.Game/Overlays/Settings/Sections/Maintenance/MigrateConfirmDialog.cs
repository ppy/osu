// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class MigrateConfirmDialog : PopupDialog
    {
        public MigrateConfirmDialog(Action action, string newLocation)
        {
            BodyText = $"迁移数据至{newLocation}?\n请确保目标路径有足够的空间!";

            Icon = FontAwesome.Solid.ExclamationTriangle;
            HeaderText = @"确认一下:";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"是的.",
                    Action = action
                },
                new PopupDialogCancelButton
                {
                    Text = @"不不不!是我点错了!",
                },
            };
        }
    }
}
