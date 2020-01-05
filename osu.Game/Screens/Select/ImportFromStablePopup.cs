// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class ImportFromStablePopup : PopupDialog
    {
        public ImportFromStablePopup(Action importFromStable)
        {
            HeaderText = @"你还没有谱面OAO!";
            BodyText = "但是我们发现了这台机器上有另一个osu!已被安装\n是否要现在导入所有的皮肤/谱面和成绩?";

            Icon = FontAwesome.Solid.Plane;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"是的,请立即开始!",
                    Action = importFromStable
                },
                new PopupDialogCancelButton
                {
                    Text = @"不了,我更喜欢从头开始",
                },
            };
        }
    }
}
