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
            HeaderText = @"你还没有任何谱面oAo!";
            BodyText = "但是我们发现你安装了另一个osu!\n你想现在导入你的所有谱面,成绩和皮肤吗?";

            Icon = FontAwesome.Solid.Plane;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"是的!现在开始吧!",
                    Action = importFromStable
                },
                new PopupDialogCancelButton
                {
                    Text = @"不,谢谢.我更喜欢从头开始",
                },
            };
        }
    }
}
