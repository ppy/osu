// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Collections
{
    public class DeleteCollectionDialog : PopupDialog
    {
        public DeleteCollectionDialog(BeatmapCollection collection, Action deleteAction)
        {
            HeaderText = "请确认是否删除以下收藏夹?";
            BodyText = $"{collection.Name.Value} ({collection.BeatmapHashes.Count}张谱面)";

            Icon = FontAwesome.Regular.TrashAlt;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"是的，我确定",
                    Action = deleteAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"不是...我点错了><！",
                },
            };
        }
    }
}
