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
            HeaderText = "Confirm deletion of";
            BodyText = $"{collection.Name.Value} ({"beatmap".ToQuantity(collection.Beatmaps.Count)})";

            Icon = FontAwesome.Regular.TrashAlt;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = @"Yes. Go for it.",
                    Action = deleteAction
                },
                new PopupDialogCancelButton
                {
                    Text = @"No! Abort mission!",
                },
            };
        }
    }
}
